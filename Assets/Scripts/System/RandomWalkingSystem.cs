using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRW<RandomWalking> randomWalking,
            //RefRW<UnitMover> unitMover,
            RefRW<TargetPositionPathQueued> targetPositionPathQueue,
            EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueueEnabled,
            RefRO<LocalTransform> localTransform)
            in SystemAPI.Query<
                RefRW<RandomWalking>,
                //RefRW<UnitMover>,
                RefRW<TargetPositionPathQueued>,
                EnabledRefRW<TargetPositionPathQueued>,
                RefRO <LocalTransform>>().WithPresent<TargetPositionPathQueued>())
        {
            float distance = math.distancesq(localTransform.ValueRO.Position, randomWalking.ValueRO.targetPosition);

            if (distance < UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ)
            {
                //reach target distance
                //更新一个随机位置
                Random random = randomWalking.ValueRO.random;

                float3 randomDirection = new float3(random.NextFloat(-1f, +1f), 0, random.NextFloat(-1f, +1f));
                randomDirection = math.normalize(randomDirection);

                randomWalking.ValueRW.targetPosition =
                    randomWalking.ValueRW.originPosition +
                    randomDirection * random.NextFloat(randomWalking.ValueRO.distanceMin, randomWalking.ValueRO.distanceMax);

                randomWalking.ValueRW.random = random;
            }
            else
            {
                //too far, move closer
                //unitMover.ValueRW.targetPosition = randomWalking.ValueRO.targetPosition;
                targetPositionPathQueue.ValueRW.targetPosition = randomWalking.ValueRW.targetPosition;
                targetPositionPathQueueEnabled.ValueRW = true;
            }
        }
    }
}
