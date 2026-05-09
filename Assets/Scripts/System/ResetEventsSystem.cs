using Unity.Burst;
using Unity.Entities;

/// <summary>
/// 确保在后期模拟系统组的最后运行
/// </summary>
[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventsSystem : ISystem
{
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
        new ResetSelectedEventsJob().ScheduleParallel();

        new ResetHealthEventsJob().ScheduleParallel();

        new ResetShootAttackEventsJob().ScheduleParallel();
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