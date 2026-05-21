using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// 确保在后期模拟系统组的最后运行
/// </summary>
[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventsSystem : ISystem
{
    private NativeArray<JobHandle> jobHandleNativeArray;

    private NativeList<Entity> onBarrakcsUnitQueueChangedEntityList;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        jobHandleNativeArray = new NativeArray<JobHandle>(5, Allocator.Persistent);
        onBarrakcsUnitQueueChangedEntityList = new NativeList<Entity>(Allocator.Persistent);
    }
     
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        #region 非Jobs
        //foreach (RefRW<Selected> selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        //{
        //    selected.ValueRW.onSelected = false;
        //    selected.ValueRW.onDeselected = false;
        //}

        //foreach (RefRW<Health> health in SystemAPI.Query<RefRW<Health>>())
        //{
        //    health.ValueRW.onHealthChange = false;
        //}

        //foreach (RefRW<ShootAttack> shootAttack in SystemAPI.Query<RefRW<ShootAttack>>())
        //{
        //    shootAttack.ValueRW.onShoot.isTriggered = false;
        //}
        #endregion


        if (SystemAPI.HasSingleton<BuildingHQ>())
        {
            Health hqHealth = SystemAPI.GetComponent<Health>(SystemAPI.GetSingletonEntity<BuildingHQ>());
            if (hqHealth.onDead)
            {
                DOTSEventsManager.Instance.TriggerOnHQDead();
            }
        }

        #region Jobs
        //串行执行
        //new ResetSelectedEventsJob().ScheduleParallel();
        //new ResetHealthEventsJob().ScheduleParallel();
        //new ResetShootAttackEventsJob().ScheduleParallel();
        //new ResetMeleeAttackEventsJob().ScheduleParallel();

        //并行执行
        jobHandleNativeArray[0] = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[1] = new ResetHealthEventsJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[2] = new ResetShootAttackEventsJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[3] = new ResetMeleeAttackEventsJob().ScheduleParallel(state.Dependency);

        //NativeList<Entity> onBarrakcsUnitQueueChangedEntityList = new NativeList<Entity>(Allocator.TempJob);
        new ResetBuildingBarracksEventsJob()
        {
            onUnitQueueChangedEntityList = onBarrakcsUnitQueueChangedEntityList.AsParallelWriter()
        }.ScheduleParallel(state.Dependency).Complete();//确保任务完成

        DOTSEventsManager.Instance.TriggerOnBarracksUnitQueueChanged(onBarrakcsUnitQueueChangedEntityList);

        state.Dependency = JobHandle.CombineDependencies(jobHandleNativeArray);
        #endregion
    }

    public void OnDestroy(ref SystemState state)
    {
        jobHandleNativeArray.Dispose();
        onBarrakcsUnitQueueChangedEntityList.Dispose();
    }
}


[BurstCompile]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ResetSelectedEventsJob : IJobEntity
{
    public void Execute(ref Selected selected)
    {
        selected.onSelected = false;
        selected.onDeselected = false;
    }
}

[BurstCompile]
public partial struct ResetHealthEventsJob : IJobEntity
{
    public void Execute(ref Health health)
    {
        health.onHealthChange = false;
        health.onDead = false;
    }
}


[BurstCompile]
public partial struct ResetShootAttackEventsJob : IJobEntity
{
    public void Execute(ref ShootAttack shootAttack)
    {
        shootAttack.onShoot.isTriggered = false;
    }
}

[BurstCompile]
public partial struct ResetMeleeAttackEventsJob : IJobEntity
{
    public void Execute(ref MeleeAttack meleeAttack)
    {
        meleeAttack.onAttacked = false;
    }
}

[BurstCompile]
public partial struct ResetBuildingBarracksEventsJob : IJobEntity
{
    public NativeList<Entity>.ParallelWriter onUnitQueueChangedEntityList;

    public void Execute(ref BuildingBarracks buildingBarracks, Entity entity)
    {
        if (buildingBarracks.onUnitQueueChanged)
        {
            buildingBarracks.onUnitQueueChanged = false;
            onUnitQueueChangedEntityList.AddNoResize(entity);
        }
    }
}