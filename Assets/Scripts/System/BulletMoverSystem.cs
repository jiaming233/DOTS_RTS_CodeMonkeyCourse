using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRW<LocalTransform> localTransform,
            RefRO<Bullet> bullet,
            RefRO<Target> target,
            Entity entity)
            in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRO<Bullet>,
                RefRO<Target>>().WithEntityAccess())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                //销毁子弹
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            ShootVictim targetShootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
            float3 targetPosition = targetLocalTransform.TransformPoint(targetShootVictim.hitLocalPosition);

            //移动前距离
            float distanceBeforeSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

            //移动后距离
            float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            if (distanceAfterSq > distanceBeforeSq)
            {
                //子弹超过目标 直接放置于目标位置
                localTransform.ValueRW.Position = targetPosition;
            }

            float destroyDistanceSq = 0.2f;

            if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSq)
            {
                //足够接近，造成伤害
                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChange = true;

                //销毁子弹
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
}
