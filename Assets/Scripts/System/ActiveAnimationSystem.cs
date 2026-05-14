using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

partial struct ActiveAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationDataHolder>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AnimationDataHolder animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

        ActiveAnimationJob activeAnimationJob = new ActiveAnimationJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            animationDataBlobArrayBlobAssetReference = animationDataHolder.animationDataBlobArrayBlobAssetReference
        };
        activeAnimationJob.ScheduleParallel();

        //foreach((
        //    RefRW<ActiveAnimation> activeAnimation,
        //    RefRW<MaterialMeshInfo> materialMeshInfo
        //    ) in SystemAPI.Query<
        //        RefRW<ActiveAnimation>, 
        //        RefRW<MaterialMeshInfo>>())
        //{

        //    //if (!activeAnimation.ValueRO.animationDataBlobAssetReference.IsCreated)
        //    //{
        //    //    activeAnimation.ValueRW.animationDataBlobAssetReference = animationDataHolder.soldierIdle;
        //    //}

        //    //if (Input.GetKeyDown(KeyCode.T))
        //    //{
        //    //    activeAnimation.ValueRW.nextAnimationType = AnimationDataSO.AnimationType.SoldierIdle;// 0;
        //    //}

        //    //if (Input.GetKeyDown(KeyCode.Y))
        //    //{
        //    //    activeAnimation.ValueRW.nextAnimationType = AnimationDataSO.AnimationType.SoldierWalk; // 1;
        //    //}

        //    //根据当前活跃动画索引访问blob数组
        //    ref AnimationData activeAnimationData =
        //        ref animationDataHolder.animationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.ValueRW.activeAnimationType];

        //    activeAnimation.ValueRW.frameTimer += SystemAPI.Time.DeltaTime;

        //    if(activeAnimation.ValueRO.frameTimer > activeAnimationData.frameTimerMax/*activeAnimation.ValueRO.animationDataBlobAssetReference.Value.frameTimerMax*/)
        //    {
        //        activeAnimation.ValueRW.frameTimer -= activeAnimationData.frameTimerMax/*activeAnimation.ValueRO.animationDataBlobAssetReference.Value.frameTimerMax*/;

        //        //帧数+1
        //        activeAnimation.ValueRW.frame = (activeAnimation.ValueRW.frame + 1) % activeAnimationData.frameMax;

        //        //////更新网格
        //        ////switch (activeAnimation.ValueRW.frame)
        //        ////{
        //        ////    default:
        //        ////    case 0:
        //        ////        materialMeshInfo.ValueRW.MeshID = activeAnimation.ValueRO.frame0;
        //        ////        break;
        //        ////    case 1:
        //        ////        materialMeshInfo.ValueRW.MeshID = activeAnimation.ValueRO.frame1;
        //        ////        break;
        //        ////}            

        //        //materialMeshInfo.ValueRW.MeshID = 
        //        //    activeAnimation.ValueRO.animationDataBlobAssetReference.Value.batchMeshIDBlobArray[activeAnimation.ValueRW.frame];

        //        materialMeshInfo.ValueRW.MeshID = activeAnimationData.batchMeshIDBlobArray[activeAnimation.ValueRW.frame];

        //        //确保射击\攻击动画播完
        //        if (activeAnimation.ValueRO.frame == 0
        //            && activeAnimation.ValueRO.activeAnimationType == AnimationDataSO.AnimationType.SoldierShoot)
        //        {
        //            activeAnimation.ValueRW.activeAnimationType = AnimationDataSO.AnimationType.None;
        //        }

        //        if (activeAnimation.ValueRO.frame == 0
        //           && activeAnimation.ValueRO.activeAnimationType == AnimationDataSO.AnimationType.ZombieAttack)
        //        {
        //            activeAnimation.ValueRW.activeAnimationType = AnimationDataSO.AnimationType.None;
        //        }
        //    }
        //}
    }
}

[BurstCompile]
public partial struct ActiveAnimationJob : IJobEntity
{
    public float deltaTime;
    public BlobAssetReference<BlobArray<AnimationData>> animationDataBlobArrayBlobAssetReference;

    public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
    {
        //根据当前活跃动画索引访问blob数组
        ref AnimationData activeAnimationData = ref animationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.activeAnimationType];

        activeAnimation.frameTimer += deltaTime;

        if (activeAnimation.frameTimer > activeAnimationData.frameTimerMax)
        {
            activeAnimation.frameTimer -= activeAnimationData.frameTimerMax;

            //帧数+1
            activeAnimation.frame = (activeAnimation.frame + 1) % activeAnimationData.frameMax;

            //materialMeshInfo.MeshID = activeAnimationData.batchMeshIDBlobArray[activeAnimation.frame];
            materialMeshInfo.Mesh = activeAnimationData.intMeshIDBlobArray[activeAnimation.frame];

            //确保射击\攻击动画播完
            if (activeAnimation.frame == 0
                && activeAnimation.activeAnimationType == AnimationDataSO.AnimationType.SoldierShoot)
            {
                activeAnimation.activeAnimationType = AnimationDataSO.AnimationType.None;
            }

            if (activeAnimation.frame == 0
               && activeAnimation.activeAnimationType == AnimationDataSO.AnimationType.ZombieAttack)
            {
                activeAnimation.activeAnimationType = AnimationDataSO.AnimationType.None;
            }
        }
    }
}