using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

/// <summary>
/// 单位移动器系统
/// </summary>
partial struct UnitMoverSystem : ISystem
{
    //[BurstCompile]
    //public void OnCreate(ref SystemState state)
    //{

    //}



    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
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

        UnitMoverJob unitMoverJob = new UnitMoverJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };

        ////作业直接在主线程上运行
        //unitMoverJob.Run();

        //把这个任务拆分到多个 CPU 核心上同时运行
        unitMoverJob.ScheduleParallel();
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
    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        float reachedTargetDistanceSq = 2f;
        if (math.lengthsq(moveDirection) < reachedTargetDistanceSq)
        {
            //reach target position
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }

        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation =
            math.slerp(localTransform.Rotation,
                quaternion.LookRotation(moveDirection, math.up()),
                deltaTime * unitMover.rotationSpeed);

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}
