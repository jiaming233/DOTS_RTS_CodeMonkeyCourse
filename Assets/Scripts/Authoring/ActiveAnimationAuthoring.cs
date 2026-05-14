using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;


public class ActiveAnimationAuthoring : MonoBehaviour
{
    //public AnimationDataSO soldierIdle;

    public AnimationDataSO.AnimationType nextAnimationType;

    public class Baker : Baker<ActiveAnimationAuthoring>
    {
        public override void Bake(ActiveAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            //EntitiesGraphicsSystem entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            //AddComponent(entity, new ActiveAnimation()
            //{
            //    frame0 = entitiesGraphicsSystem.RegisterMesh(authoring.soldierIdle.meshArray[0]),
            //    frame1 = entitiesGraphicsSystem.RegisterMesh(authoring.soldierIdle.meshArray[1]),
            //    frameMax = authoring.soldierIdle.meshArray.Length,
            //    frameTimerMax = authoring.soldierIdle.frameTimerMax
            //});

            AddComponent(entity, new ActiveAnimation()
            {
                nextAnimationType = authoring.nextAnimationType
            });
        }
    }
}

public struct ActiveAnimation : IComponentData
{
    /// <summary>
    /// 当前帧
    /// </summary>
    public int frame;

    /// <summary>
    /// 当前计时器
    /// </summary>
    public float frameTimer;

    //public BlobAssetReference<AnimationData> animationDataBlobAssetReference;
    /// <summary>
    /// 活动动画索引
    /// </summary>
    //public int activeAnimationIndex;
    public AnimationDataSO.AnimationType activeAnimationType;

    public AnimationDataSO.AnimationType nextAnimationType;
}