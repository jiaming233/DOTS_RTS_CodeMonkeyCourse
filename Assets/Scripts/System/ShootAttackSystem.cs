using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct ShootAttackSystem : ISystem
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

        #region 单位
        foreach ((
           RefRW<LocalTransform> localTransform,
           RefRW<ShootAttack> shootAttack,
           RefRO<Target> target,
           RefRW<UnitMover> unitMover,
           Entity entity)
           in SystemAPI.Query<
               RefRW<LocalTransform>,
               RefRW<ShootAttack>,
               RefRO<Target>,
               RefRW<UnitMover>>().WithDisabled<MoveOverride>().WithEntityAccess())//仅在移动覆盖组件禁用时 执行此逻辑
        {
            if(target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            RefRO<LocalTransform> targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.targetEntity);

            float distance = math.distance(targetLocalTransform.ValueRO.Position, localTransform.ValueRO.Position);

            if (distance > shootAttack.ValueRO.attackDistance)
            {
                //too far to attack, move closer
                unitMover.ValueRW.targetPosition = targetLocalTransform.ValueRO.Position;
                continue;
            }
            else
            {
                //stop moving then attack
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            }

            float3 aimDirection = targetLocalTransform.ValueRO.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);

            quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, 
                SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);

            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (shootAttack.ValueRO.timer > 0f)
            {
                //计时未结束
                continue;
            }
            shootAttack.ValueRW.timer = shootAttack.ValueRW.timerMax;

            //生成子弹
            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);

            //局部坐标转换为世界坐标
            float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
            //设置子弹实体的位置
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition/*localTransform.ValueRO.Position*/));

            //设置子弹实体的子弹组件
            RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bulletBullet.ValueRW.damageAmount = shootAttack.ValueRO.damageAmount;

            //设置子弹实体的目标组件
            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;

            shootAttack.ValueRW.onShoot.isTriggered = true;
            shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;

            if (SystemAPI.HasComponent<TargetOverride>(target.ValueRO.targetEntity))
            {
                //将被攻击僵尸的目标覆盖为当前射击的实体
                RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.targetEntity);
                if (enemyTargetOverride.ValueRO.targetEntity == Entity.Null)
                {
                    enemyTargetOverride.ValueRW.targetEntity = entity;
                }
            }
        }
        #endregion


        #region 防御塔
        foreach ((
         RefRW<LocalTransform> localTransform,
         RefRW<ShootAttack> shootAttack,
         RefRO<Target> target,
         Entity entity)
         in SystemAPI.Query<
             RefRW<LocalTransform>,
             RefRW<ShootAttack>,
             RefRO<Target>>().WithEntityAccess())//仅在移动覆盖组件禁用时 执行此逻辑
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            RefRO<LocalTransform> targetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(target.ValueRO.targetEntity);

            if(math.distance(localTransform.ValueRO.Position, targetLocalTransform.ValueRO.Position) > shootAttack.ValueRO.attackDistance)
            {
                continue;
            }

            if(SystemAPI.HasComponent<MoveOverride>(entity) && SystemAPI.IsComponentEnabled<MoveOverride>(entity))
            {
                //moveoverride is enabled
                continue;
            }

            shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (shootAttack.ValueRO.timer > 0f)
            {
                //计时未结束
                continue;
            }
            shootAttack.ValueRW.timer = shootAttack.ValueRW.timerMax;

            //生成子弹
            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefabEntity);

            //局部坐标转换为世界坐标
            float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
            //设置子弹实体的位置
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition/*localTransform.ValueRO.Position*/));

            //设置子弹实体的子弹组件
            RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bulletBullet.ValueRW.damageAmount = shootAttack.ValueRO.damageAmount;

            //设置子弹实体的目标组件
            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;

            shootAttack.ValueRW.onShoot.isTriggered = true;
            shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;

            if (SystemAPI.HasComponent<TargetOverride>(target.ValueRO.targetEntity))
            {
                //将被攻击僵尸的目标覆盖为当前射击的实体
                RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.targetEntity);
                if (enemyTargetOverride.ValueRO.targetEntity == Entity.Null)
                {
                    enemyTargetOverride.ValueRW.targetEntity = entity;
                }
            }
        }
        #endregion
    }
}
