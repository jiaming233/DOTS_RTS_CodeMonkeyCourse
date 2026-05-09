using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((
           RefRW<LocalTransform> localTransform,
           RefRW<MeleeAttack> meleeAttack,
           RefRO<Target> target,
           RefRW<UnitMover> unitMover)
           in SystemAPI.Query<
               RefRW<LocalTransform>,
               RefRW<MeleeAttack>,
               RefRO<Target>,
               RefRW<UnitMover>>().WithDisabled<MoveOverride>())//仅在移动覆盖组件禁用时 执行此逻辑)
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

            float meleeAttackDistanceSq = 2f;

            // 仅通过距离判断会受攻击目标碰撞体大小影响
            bool isCloseEnoughToAttack = math.distancesq(targetLocalTransform.Position, localTransform.ValueRO.Position) < meleeAttackDistanceSq;
            // 结合射线检测
            bool isTouchingTarget = false;

            if (!isCloseEnoughToAttack)
            {
                float3 dirToTarget = targetLocalTransform.Position - localTransform.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);
                //射线长度略大于本体collider的大小
                float distanceExtraToTestRaycast = 0.4f;

                RaycastInput raycastInput = new RaycastInput()
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position + dirToTarget * (meleeAttack.ValueRO.colliderSize + distanceExtraToTestRaycast),
                    Filter = CollisionFilter.Default
                };

                raycastHitList.Clear();
                if (collisionWorld.CastRay(raycastInput, ref raycastHitList))
                {
                    //hit target
                    foreach (RaycastHit hit in raycastHitList)
                    {
                        if (hit.Entity == target.ValueRO.targetEntity)
                        {
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }

            if (!isCloseEnoughToAttack && !isTouchingTarget)
            {
                //too far to attack, move closer
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
            }
            else
            {
                //stop moving then attack
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;

                float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
                aimDirection = math.normalize(aimDirection);

                quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
                localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation,
                    SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);


                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (meleeAttack.ValueRO.timer > 0f)
                {
                    //计时未结束
                    continue;
                }
                meleeAttack.ValueRW.timer = meleeAttack.ValueRW.timerMax;


                //造成伤害
                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChange = true;
            }      
        }
    }
}
