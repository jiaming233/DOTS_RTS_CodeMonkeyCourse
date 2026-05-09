using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Ray = UnityEngine.Ray;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    //屏幕坐标
    private Vector2 selectionStartMousePosition;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            //Vector2 selectionEndMousePosition = Input.mousePosition;

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            #region 取消选择所有单位
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);

            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);

                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                entityManager.SetComponentData<Selected>(entityArray[i], selected);
            }
            #endregion

            #region 选择区域内单位\点击单选
            entityQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform, Unit>()
                .WithPresent<Selected>()
                .Build(entityManager);

            entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            Rect selectionAreaRect = GetSelectionAreaRect();
            //选择区域大小, 判断是否进行框选多个单位
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            if (isMultipleSelection)
            {
                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransformArray[i];
                    //获取单位屏幕位置
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                    //判断是否在选择区
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);

                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData<Selected>(entityArray[i], selected);
                    }
                }
            }
            else
            {
                //单选

                //等价查询
                //new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>().Build(entityManager);
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                //获取单例
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();

                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastInput raycastInput = new RaycastInput()
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter()
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER,//Layer:Units 位掩码
                        GroupIndex = 0
                    }
                };

                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        //hit a unit
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);

                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData<Selected>(raycastHit.Entity, selected);
                    }
                }
            }
            #endregion

            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }


        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;


            #region 是否点击敌人
            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));

            PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastInput raycastInput = new RaycastInput()
            {
                Start = cameraRay.GetPoint(0f),
                End = cameraRay.GetPoint(9999f),
                Filter = new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.UNITS_LAYER,//Layer:Units 位掩码
                    GroupIndex = 0
                }
            };

            bool isAttackingSingleTarget = false;
            if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
            {
                if (entityManager.HasComponent<Unit>(raycastHit.Entity))
                {
                    Unit unit = entityManager.GetComponentData<Unit>(raycastHit.Entity);
                    //右键点击僵尸
                    if (unit.faction == Faction.Zombie)
                    {
                        isAttackingSingleTarget = true;

                        //查找目标覆盖                      
                        entityQuery = new EntityQueryBuilder(Allocator.Temp)
                            .WithAll<UnitMover, Selected>()
                            .WithPresent<TargetOverride>()
                            .Build(entityManager);

                        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                        NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                        for (int i = 0; i < targetOverrideArray.Length; i++)
                        {
                            TargetOverride targetOverride = targetOverrideArray[i];
                            targetOverride.targetEntity = raycastHit.Entity;
                            targetOverrideArray[i] = targetOverride;

                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                        }
                        //为所有选中单位设置目标覆盖
                        entityQuery.CopyFromComponentDataArray(targetOverrideArray);
                    }
                }
            }
            #endregion

            if (!isAttackingSingleTarget)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<UnitMover, Selected>()
                    .WithPresent<MoveOverride>()
                    .WithPresent<TargetOverride>()
                    .Build(entityManager);

                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                //NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
                //移动覆盖
                NativeArray<MoveOverride> moveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
                //目标覆盖
                NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                #region 单个更新
                //for (int i = 0; i < unitMoverArray.Length; i++)
                //{
                //    //副本 未修改存储在实体中的数据
                //    UnitMover unitMover = unitMoverArray[i];
                //    unitMover.targetPosition = mouseWorldPosition;
                //    //更新实体的数据
                //    entityManager.SetComponentData(entityArray[i], unitMover);
                //}
                #endregion

                #region 批量更新
                NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);

                for (int i = 0; i < moveOverrideArray.Length; i++)
                {
                    //副本 未修改存储在实体中的数据
                    MoveOverride moveOverride = moveOverrideArray[i];
                    moveOverride.targetPosition = /*mouseWorldPosition*/movePositionArray[i];
                    moveOverrideArray[i] = moveOverride;

                    TargetOverride targetOverride = targetOverrideArray[i];
                    targetOverride.targetEntity = Entity.Null;
                    targetOverrideArray[i] = targetOverride;

                    entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);
                }

                entityQuery.CopyFromComponentDataArray(moveOverrideArray);
                entityQuery.CopyFromComponentDataArray(targetOverrideArray);
                #endregion
            }
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;

        Vector2 lowerLeftCorner = new Vector2(Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y));

        Vector2 upperRightCorner = new Vector2(Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y));

        return new Rect(
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y);
    }

    public NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);
        if (positionCount == 0)
        {
            return positionArray;
        }

        positionArray[0] = targetPosition;
        if (positionCount == 1)
        {
            return positionArray;
        }

        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        while (positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;

            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = math.PI2 / ringPositionCount;
                float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ring + 1), 0, 0));
                float3 ringPosition = targetPosition + ringVector;

                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount)
                {
                    break;
                }
            }

            ring++;
        }

        return positionArray;
    }
}
