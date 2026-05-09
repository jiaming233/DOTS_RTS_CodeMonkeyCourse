using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootLightSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRO<ShootAttack> shootAttack)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRO<ShootAttack>>())
        {
            if (shootAttack.ValueRO.onShoot.isTriggered)
            {
                //Éú³ÉµÆ¹â
                Entity shootLightEntity = state.EntityManager.Instantiate(entitiesReferences.shootLightPrefabEntity);

                SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shootAttack.ValueRO.onShoot.shootFromPosition));
            }
        }
    }
}
