using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

/// <summary>
/// 单位移动器系统
/// </summary>
partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;

    public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueueComponentLookup;
    public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;
    public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    public ComponentLookup<MoveOverride> moveOverrideComponentLookup;
    public ComponentLookup<GridSystem.GridNode> gridNodeComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridSystem.GridSystemData>();

        targetPositionPathQueueComponentLookup = SystemAPI.GetComponentLookup<TargetPositionPathQueued>(false);
        flowFieldPathRequestComponentLookup = SystemAPI.GetComponentLookup<FlowFieldPathRequest>(false);
        flowFieldFollowerComponentLookup = SystemAPI.GetComponentLookup<FlowFieldFollower>(false);
        moveOverrideComponentLookup = SystemAPI.GetComponentLookup<MoveOverride>(false);
        gridNodeComponentLookup = SystemAPI.GetComponentLookup<GridSystem.GridNode>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridSystem.GridSystemData gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

        targetPositionPathQueueComponentLookup.Update(ref state);
        flowFieldPathRequestComponentLookup.Update(ref state);
        flowFieldFollowerComponentLookup.Update(ref state);
        moveOverrideComponentLookup.Update(ref state);
        gridNodeComponentLookup.Update(ref state);

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        #region 优化 单位当前位置和目标位置之间没有障碍物时，直接移动到目标位置，否则启用流场寻路，计算流场路径
        TargetPositionPathQueuedJob targetPositionPathQueuedJob = new TargetPositionPathQueuedJob()
        {
            collisionWorld = collisionWorld,
            targetPositionPathQueueComponentLookup = targetPositionPathQueueComponentLookup,
            flowFieldPathRequestComponentLookup = flowFieldPathRequestComponentLookup,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
            moveOverrideComponentLookup = moveOverrideComponentLookup,
            width = gridSystemData.width,
            height = gridSystemData.height,
            gridNodeSize = gridSystemData.gridNodeSize,
            costMap = gridSystemData.costMap
        };
        targetPositionPathQueuedJob.ScheduleParallel();
        #region 主线程
        //foreach ((
        //  RefRO<LocalTransform> localTransform,
        //  RefRW<TargetPositionPathQueued> targetPositionPathQueue,
        //  EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueueEnabled,
        //  RefRW<FlowFieldPathRequest> flowFieldPathRequest,
        //  EnabledRefRW<FlowFieldPathRequest> flowFieldPathRequestEnabled,
        //  EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled,
        //  RefRW<UnitMover> unitMover,
        //  Entity entity)
        //      in SystemAPI.Query<
        //          RefRO<LocalTransform>,
        //          RefRW<TargetPositionPathQueued>,
        //          EnabledRefRW<TargetPositionPathQueued>,
        //          RefRW<FlowFieldPathRequest>,
        //          EnabledRefRW<FlowFieldPathRequest>,
        //          EnabledRefRW<FlowFieldFollower>,
        //          RefRW<UnitMover>>().WithPresent<FlowFieldPathRequest, FlowFieldFollower>().WithEntityAccess())
        //{
        //    RaycastInput raycastInput = new RaycastInput()
        //    {
        //        Start = localTransform.ValueRO.Position,
        //        End = targetPositionPathQueue.ValueRO.targetPosition,
        //        Filter = new CollisionFilter()
        //        {
        //            BelongsTo = ~0u,
        //            CollidesWith = 1u << GameAssets.PATHFINDING_WALLS_LAYER,
        //            GroupIndex = 0
        //        }
        //    };
        //    if (!collisionWorld.CastRay(raycastInput))
        //    {
        //        //did not hit any walls
        //        unitMover.ValueRW.targetPosition = targetPositionPathQueue.ValueRO.targetPosition;

        //        flowFieldPathRequestEnabled.ValueRW = false;
        //        flowFieldFollowerEnabled.ValueRW = false;
        //    }
        //    else
        //    {
        //        //there's wall in between
        //        if (SystemAPI.HasComponent<MoveOverride>(entity))
        //        {
        //            SystemAPI.SetComponentEnabled<MoveOverride>(entity, false);
        //        }
        //        //启用流场路径请求组件，计算流场
        //        if (GridSystem.IsValidWalkableGridPosition(targetPositionPathQueue.ValueRO.targetPosition, gridSystemData))
        //        {
        //            flowFieldPathRequest.ValueRW.targetPosition = targetPositionPathQueue.ValueRO.targetPosition;
        //            flowFieldPathRequestEnabled.ValueRW = true;
        //        }
        //        else
        //        {
        //            unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

        //            flowFieldPathRequestEnabled.ValueRW = false;
        //            flowFieldFollowerEnabled.ValueRW = false;
        //        }
        //    }
        //    targetPositionPathQueueEnabled.ValueRW = false;
        //}
        #endregion
        #endregion

        #region  处理FlowFieldFollower使其根据路径，同时射线检测是否有障碍物，如果没有障碍物则直接移动到目标位置
        TestCanMoveStraightJob testCanMoveStraightJob = new TestCanMoveStraightJob()
        {
            collisionWorld = collisionWorld,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup
        };
        testCanMoveStraightJob.ScheduleParallel();

        FlowFieldFollowerJob flowFieldFollowerJob = new FlowFieldFollowerJob()
        {
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
            gridNodeComponentLookup = gridNodeComponentLookup,
            width = gridSystemData.width,
            height = gridSystemData.height,
            gridNodeSize = gridSystemData.gridNodeSize,
            gridNodeSizeDouble = gridSystemData.gridNodeSize * 2f,
            totalGridMapEntityArray = gridSystemData.totalGridMapEntityArray
        };
        flowFieldFollowerJob.ScheduleParallel();
        #region 主线程
        //foreach ((
        //    RefRO<LocalTransform> localTransform,
        //    RefRW<FlowFieldFollower> flowFieldFollower,
        //    EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled,
        //    RefRW<UnitMover> unitMover)
        //    in SystemAPI.Query<
        //        RefRO<LocalTransform>,
        //        RefRW<FlowFieldFollower>,
        //        EnabledRefRW<FlowFieldFollower>,
        //        RefRW<UnitMover>>())
        //{
        //    int2 gridPosition = GridSystem.GetGridPosition(localTransform.ValueRO.Position, gridSystemData.gridNodeSize);
        //    if (GridSystem.IsValidGridPosition(gridPosition, gridSystemData.width, gridSystemData.height))
        //    {
        //        int index = GridSystem.CalculateIndex(gridPosition.x, gridPosition.y, gridSystemData.width);
        //        Entity gridNodeEntity = gridSystemData.gridMapArray[flowFieldFollower.ValueRO.gridIndex].gridEntityArray[index];
        //        GridSystem.GridNode gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridNodeEntity);
        //        float3 gridNodeMoveVector = GridSystem.GetWorldMovementVector(gridNode.vector);

        //        if (GridSystem.IsWall(gridNode))
        //        {
        //            gridNodeMoveVector = flowFieldFollower.ValueRO.lastMoveVector;
        //        }
        //        else
        //        {
        //            flowFieldFollower.ValueRW.lastMoveVector = gridNodeMoveVector;
        //        }

        //        unitMover.ValueRW.targetPosition =
        //            GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridSystemData.gridNodeSize)
        //            + gridNodeMoveVector * gridSystemData.gridNodeSize * 2f;

        //        if (math.distance(localTransform.ValueRO.Position, flowFieldFollower.ValueRO.targetPosition) < gridSystemData.gridNodeSize)
        //        {
        //            unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
        //            flowFieldFollowerEnabled.ValueRW = false;
        //        }

        //        //跟随流场移动过程中一旦和目标位置之间没有障碍，就直接移动（禁用FlowFieldFollower）
        //        RaycastInput raycastInput = new RaycastInput()
        //        {
        //            Start = localTransform.ValueRO.Position,
        //            End = flowFieldFollower.ValueRO.targetPosition,
        //            Filter = new CollisionFilter()
        //            {
        //                BelongsTo = ~0u,
        //                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS_LAYER,
        //                GroupIndex = 0
        //            }
        //        };
        //        if (!collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit))
        //        {
        //            //did not hit any walls
        //            unitMover.ValueRW.targetPosition = flowFieldFollower.ValueRO.targetPosition;
        //            flowFieldFollowerEnabled.ValueRW = false;
        //        }
        //    }
        //}
        #endregion
        #endregion

        UnitMoverJob unitMoverJob = new UnitMoverJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        ////作业直接在主线程上运行
        //unitMoverJob.Run();
        //把这个任务拆分到多个 CPU 核心上同时运行
        unitMoverJob.ScheduleParallel();

        #region 主线程 移动
        //foreach((
        //    RefRW<LocalTransform> localTransform, 
        //    RefRW<UnitMover> unitMover,
        //    RefRW<PhysicsVelocity> physicsVelocity)
        //    in SystemAPI.Query<
        //        RefRW<LocalTransform>, 
        //        RefRW<UnitMover>,
        //        RefRW<PhysicsVelocity>>())
        //{
        //    //localTransform.ValueRW.Position = localTransform.ValueRO.Position + new float3(unitMover.ValueRO.value, 0, 0) * SystemAPI.Time.DeltaTime;

        //    //float3 targetPosition = localTransform.ValueRO.Position + new float3(10, 0, 0);
        //    //float3 targetPosition = MouseWorldPosition.Instance.GetPosition();

        //    float3 moveDirection = unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position;
        //    moveDirection = math.normalize(moveDirection);

        //    localTransform.ValueRW.Rotation = 
        //        math.slerp(localTransform.ValueRO.Rotation, 
        //            quaternion.LookRotation(moveDirection, math.up()), 
        //            SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

        //    //localTransform.ValueRW.Position += moveDirection * unitMover.ValueRO.value * SystemAPI.Time.DeltaTime;    
        //    //物理移动
        //    physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
        //    physicsVelocity.ValueRW.Angular = float3.zero;
        //}
        #endregion
    }

    //[BurstCompile]
    //public void OnDestroy(ref SystemState state)
    //{

    //}
}

/// <summary>
/// 单位移动作业
/// IJobEntity替代传统foreach:SystemAPI.Query
/// 
/// partial是必须的
/// 因为 DOTS 的源生成器（Source Generator）需要在编译时自动生成一些样板代码，以支持高效的查询和调度
/// </summary>
[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;

    /// <summary>
    /// 只需要在 Execute 方法的参数中列出你需要的组件，系统就会自动找到所有匹配的实体，并为每个实体调用一次 Execute 方法
    /// </summary>
    /// <param name="localTransform"></param>
    /// <param name="unitMover"></param>
    /// <param name="physicsVelocity"></param>
    public void Execute(ref LocalTransform localTransform, ref UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        if (math.lengthsq(moveDirection) < UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ)
        {
            //reach target position
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            unitMover.IsMoving = false;
            return;
        }

        unitMover.IsMoving = true;

        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation =
            math.slerp(localTransform.Rotation,
                quaternion.LookRotation(moveDirection, math.up()),
                deltaTime * unitMover.rotationSpeed);

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}

[BurstCompile]
[WithAll(typeof(TargetPositionPathQueued))]
public partial struct TargetPositionPathQueuedJob : IJobEntity
{
    [ReadOnly] public CollisionWorld collisionWorld;
    [NativeDisableParallelForRestriction] public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueueComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<MoveOverride> moveOverrideComponentLookup;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public float gridNodeSize;
    [ReadOnly] public NativeArray<byte> costMap;

    public void Execute(
          in LocalTransform localTransform,
          ref UnitMover unitMover,
          Entity entity)
    {
        //这里可以处理目标位置队列的逻辑，比如检查是否有新的目标位置需要处理
        //如果有新的目标位置，可以将其设置为流场路径请求的目标位置
        //并启用流场路径请求和流场跟随组件
        RaycastInput raycastInput = new RaycastInput()
        {
            Start = localTransform.Position,
            End = targetPositionPathQueueComponentLookup[entity].targetPosition,
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS_LAYER,
                GroupIndex = 0
            }
        };
        if (!collisionWorld.CastRay(raycastInput))
        {
            //did not hit any walls
            unitMover.targetPosition = targetPositionPathQueueComponentLookup[entity].targetPosition;

            flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }
        else
        {
            //there's wall in between
            if (moveOverrideComponentLookup.HasComponent(entity))
            {
                moveOverrideComponentLookup.SetComponentEnabled(entity, false);
            }
           
            //启用流场路径请求组件，计算流场
            if (GridSystem.IsValidWalkableGridPosition(
                targetPositionPathQueueComponentLookup[entity].targetPosition,
                width, height, gridNodeSize, costMap))
            {
                FlowFieldPathRequest flowFieldPathRequest = flowFieldPathRequestComponentLookup[entity];
                flowFieldPathRequest.targetPosition = targetPositionPathQueueComponentLookup[entity].targetPosition;
                flowFieldPathRequestComponentLookup[entity] = flowFieldPathRequest;
                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, true);
            }
            else
            {
                unitMover.targetPosition = localTransform.Position;

                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
                flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
            }
        }

        targetPositionPathQueueComponentLookup.SetComponentEnabled(entity, false);
    }
}

[BurstCompile]
[WithAll(typeof(FlowFieldFollower))]
public partial struct TestCanMoveStraightJob : IJobEntity
{
    [ReadOnly] public CollisionWorld collisionWorld;
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;

    public void Execute(
        in LocalTransform localTransform,
        ref UnitMover unitMover,
        Entity entity)
    {
        FlowFieldFollower flowFieldFollower = flowFieldFollowerComponentLookup[entity];

        RaycastInput raycastInput = new RaycastInput()
        {
            Start = localTransform.Position,
            End = flowFieldFollower.targetPosition,
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS_LAYER,
                GroupIndex = 0
            }
        };
        if (!collisionWorld.CastRay(raycastInput))
        {
            //did not hit any walls
            unitMover.targetPosition = flowFieldFollower.targetPosition;
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }
    }
}

[BurstCompile]
[WithAll(typeof(FlowFieldFollower))]
public partial struct FlowFieldFollowerJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    [ReadOnly] public ComponentLookup<GridSystem.GridNode> gridNodeComponentLookup;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public float gridNodeSize;
    [ReadOnly] public float gridNodeSizeDouble;
    [ReadOnly] public NativeArray<Entity> totalGridMapEntityArray;

    public void Execute(
        in LocalTransform localTransform,
        ref UnitMover unitMover,
        Entity entity)
    {
        FlowFieldFollower flowFieldFollower = flowFieldFollowerComponentLookup[entity];

        int2 gridPosition = GridSystem.GetGridPosition(localTransform.Position, gridNodeSize);
        if (GridSystem.IsValidGridPosition(gridPosition, width, height))
        {
            int index = GridSystem.CalculateIndex(gridPosition.x, gridPosition.y, width);
            //Entity gridNodeEntity = gridSystemData.gridMapArray[flowFieldFollower.ValueRO.gridIndex].gridEntityArray[index];
            //GridSystem.GridNode gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridNodeEntity);
            int totalCount = width * height;
            Entity gridNodeEntity = totalGridMapEntityArray[totalCount * flowFieldFollower.gridIndex + index];
            GridSystem.GridNode gridNode = gridNodeComponentLookup[gridNodeEntity];

            float3 gridNodeMoveVector = GridSystem.GetWorldMovementVector(gridNode.vector);

            if (GridSystem.IsWall(gridNode))
            {
                gridNodeMoveVector = flowFieldFollower.lastMoveVector;
            }
            else
            {
                flowFieldFollower.lastMoveVector = gridNodeMoveVector;
                flowFieldFollowerComponentLookup[entity] = flowFieldFollower;
            }

            unitMover.targetPosition =
                GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridNodeSize)
                + gridNodeMoveVector * gridNodeSizeDouble;

            if (math.distance(localTransform.Position, flowFieldFollower.targetPosition) < gridNodeSize)
            {
                unitMover.targetPosition = localTransform.Position;
                flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
            }
        }
    }
}