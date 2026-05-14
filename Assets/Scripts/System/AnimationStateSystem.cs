using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


[UpdateAfter(typeof(ShootAttackSystem))]
partial struct AnimationStateSystem : ISystem
{
    private ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        activeAnimationComponentLookup = state.GetComponentLookup<ActiveAnimation>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        activeAnimationComponentLookup.Update(ref state);
        new IdleWalkingAnimationStateJob()
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup
        }.ScheduleParallel();

        activeAnimationComponentLookup.Update(ref state);
        new ShootAnimationStateJob()
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup
        }.ScheduleParallel();

        activeAnimationComponentLookup.Update(ref state);
        new MeleeAttackAnimationStateJob()
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup
        }.ScheduleParallel();

        ////idle/walk动画
        //foreach ((
        // RefRO<AnimatedMesh> animatedMesh,
        // RefRO<UnitMover> unitMover,
        // RefRO<UnitAnimations> unitAnimations
        // ) in SystemAPI.Query<
        //     RefRO<AnimatedMesh>,
        //     RefRO<UnitMover>,
        //     RefRO<UnitAnimations>>())
        //{
        //    RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);

        //    ////硬编码
        //    ////解决：添加 UnitAnimationsAuthoring
        //    //if (unitMover.ValueRO.IsMoving)
        //    //{
        //    //    activeAnimation.ValueRW.nextAnimationType = AnimationDataSO.AnimationType.SoldierWalk;
        //    //}
        //    //else
        //    //{
        //    //    activeAnimation.ValueRW.nextAnimationType = AnimationDataSO.AnimationType.SoldierIdle;
        //    //}

        //    if (unitMover.ValueRO.IsMoving)
        //    {
        //        activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.walkAnimationType;
        //    }
        //    else
        //    {
        //        activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.idleAnimationType;
        //    }
        //}

        ////射击动画
        //foreach ((
        //   RefRO<AnimatedMesh> animatedMesh,
        //   RefRO<UnitAnimations> unitAnimations,
        //   RefRO<UnitMover> unitMover,
        //   RefRO<Target> target,
        //   RefRO<ShootAttack> shootAttack)
        //   in SystemAPI.Query<
        //         RefRO<AnimatedMesh>,
        //         RefRO<UnitAnimations>,
        //         RefRO<UnitMover>,
        //         RefRO<Target>,
        //         RefRO<ShootAttack>>())
        //{
        //    if (!unitMover.ValueRO.IsMoving && target.ValueRO.targetEntity != Entity.Null)
        //    {
        //        RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
        //        activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.aimAnimationType;
        //    }

        //    if (shootAttack.ValueRO.onShoot.isTriggered)
        //    {
        //        RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
        //        activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.shootAnimationType;
        //    }
        //}

        ////近战攻击动画
        //foreach ((
        //  RefRO<AnimatedMesh> animatedMesh,
        //  RefRO<UnitAnimations> unitAnimations,
        //  RefRO<UnitMover> unitMover,
        //  RefRO<Target> target,
        //  RefRO<MeleeAttack> meleeAttack)
        //  in SystemAPI.Query<
        //        RefRO<AnimatedMesh>,
        //        RefRO<UnitAnimations>,
        //        RefRO<UnitMover>,
        //        RefRO<Target>,
        //        RefRO<MeleeAttack>>())
        //{
        //    if (meleeAttack.ValueRO.onAttacked)
        //    {
        //        RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.meshEntity);
        //        activeAnimation.ValueRW.nextAnimationType = unitAnimations.ValueRO.meleeAttackAnimationType;
        //    }
        //}
    }
}


[BurstCompile]
public partial struct IdleWalkingAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;

    public void Execute(in AnimatedMesh animatedMesh, in UnitMover unitMover, in UnitAnimations unitAnimations)
    {
        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);

        if (unitMover.IsMoving)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.walkAnimationType;
        }
        else
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.idleAnimationType;
        }
    }
}

[BurstCompile]
public partial struct ShootAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;

    public void Execute(in AnimatedMesh animatedMesh, in UnitMover unitMover, in UnitAnimations unitAnimations, in Target target, in ShootAttack shootAttack)
    {
        if (!unitMover.IsMoving && target.targetEntity != Entity.Null)
        {
            RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.aimAnimationType;
        }

        if (shootAttack.onShoot.isTriggered)
        {
            RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.shootAnimationType;
        }
    }
}

[BurstCompile]
public partial struct MeleeAttackAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;

    public void Execute(in AnimatedMesh animatedMesh, in UnitMover unitMover, in UnitAnimations unitAnimations, in Target target, in MeleeAttack meleeAttack)
    {
        if (meleeAttack.onAttacked)
        {
            RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.meleeAttackAnimationType;
        }
    }
}