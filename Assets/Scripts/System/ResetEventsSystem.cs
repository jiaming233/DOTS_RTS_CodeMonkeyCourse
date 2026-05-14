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

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        jobHandleNativeArray = new NativeArray<JobHandle>(4, Allocator.Persistent);
    }
     
    [BurstCompile]
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

        #region Jobs
        //new ResetSelectedEventsJob().ScheduleParallel();
        //new ResetHealthEventsJob().ScheduleParallel();
        //new ResetShootAttackEventsJob().ScheduleParallel();
        //new ResetMeleeAttackEventsJob().ScheduleParallel();


        jobHandleNativeArray[0] = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[1] = new ResetHealthEventsJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[2] = new ResetShootAttackEventsJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[3] = new ResetMeleeAttackEventsJob().ScheduleParallel(state.Dependency);

        state.Dependency = JobHandle.CombineDependencies(jobHandleNativeArray);
        #endregion
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