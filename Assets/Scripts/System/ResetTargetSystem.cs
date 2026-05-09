using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

//[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct ResetTargetSystem : ISystem
{
    private EntityStorageInfoLookup entityStorageInfoLookup;

    private ComponentLookup<LocalTransform> localTransformComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        #region 非Jobs
        //foreach(RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        //{
        //    if(target.ValueRO.targetEntity != Entity.Null)
        //    {
        //        if (!SystemAPI.Exists(target.ValueRO.targetEntity)
        //            || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.targetEntity))
        //        {
        //            target.ValueRW.targetEntity = Entity.Null;
        //        }
        //    }
        //}

        //foreach (RefRW<TargetOverride> targetOverride in SystemAPI.Query<RefRW<TargetOverride>>())
        //{
        //    if (targetOverride.ValueRO.targetEntity != Entity.Null)
        //    {
        //        if (!SystemAPI.Exists(targetOverride.ValueRO.targetEntity)
        //            || !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRO.targetEntity))
        //        {
        //            targetOverride.ValueRW.targetEntity = Entity.Null;
        //        }
        //    }
        //}
        #endregion

        #region Jobs
        //确保获取到的是当前这一帧最新的内存布局信息
        entityStorageInfoLookup.Update(ref state);
        localTransformComponentLookup.Update(ref state);

        new ResetTargetJob()
        {
            entityStorageInfoLookup = entityStorageInfoLookup,
            localTransformComponentLookup = localTransformComponentLookup
        }.ScheduleParallel();

        new ResetTargetOverrideJob()
        {
            entityStorageInfoLookup = entityStorageInfoLookup,
            localTransformComponentLookup = localTransformComponentLookup
        }.ScheduleParallel();
        #endregion
    }
}


[BurstCompile]
public partial struct ResetTargetJob : IJobEntity
{
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;


    public void Execute(ref Target target)
    {
        if (target.targetEntity != Entity.Null)
        {
            if (!entityStorageInfoLookup.Exists(target.targetEntity)
                || !localTransformComponentLookup.HasComponent(target.targetEntity))
            {
                target.targetEntity = Entity.Null;
            }
        }
    }
}

[BurstCompile]
public partial struct ResetTargetOverrideJob : IJobEntity
{
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;

    public void Execute(ref TargetOverride targetOverride)
    {
        if (targetOverride.targetEntity != Entity.Null)
        {
            if (!entityStorageInfoLookup.Exists(targetOverride.targetEntity)
                || !localTransformComponentLookup.HasComponent(targetOverride.targetEntity))
            {
                targetOverride.targetEntity = Entity.Null;
            }
        }
    }
}