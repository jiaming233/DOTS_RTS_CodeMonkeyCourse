using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemyAttackHQSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BuildingHQ>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity hqEntity = SystemAPI.GetSingletonEntity<BuildingHQ>();
        float3 hqPosition = SystemAPI.GetComponent<LocalTransform>(hqEntity).Position;

        foreach((
            RefRW<EnemyAttackHQ> enemyAttackHQ,
            RefRW<UnitMover> unitMover,
            RefRO<Target> target)
            in SystemAPI.Query<
                RefRW<EnemyAttackHQ>, 
                RefRW<UnitMover>, 
                RefRO<Target>>().WithDisabled<MoveOverride>())//移动覆盖启用时 不执行此逻辑，优先攻击其它有效目标
        {
            if(target.ValueRO.targetEntity != Entity.Null)
            {
                continue;
            }

            unitMover.ValueRW.targetPosition = hqPosition;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
