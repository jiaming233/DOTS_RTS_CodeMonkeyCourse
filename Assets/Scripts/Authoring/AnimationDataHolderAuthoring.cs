using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationDataHolderAuthoring : MonoBehaviour
{
    //public AnimationDataSO soldierIdle;
    //public AnimationDataSO soldierWalk;
    public AnimationDataListSO animationDataListSO;

    public Material defaultMaterial;

    public class Baker : Baker<AnimationDataHolderAuthoring>
    {
        public override void Bake(AnimationDataHolderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            /*
            ///子场景关闭时 会发生错误
            EntitiesGraphicsSystem entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            */
            AnimationDataHolder animationDataHolder = new AnimationDataHolder();

            #region 硬编码
            ////build soldierIdle blob
            //{
            //    BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            //    //ref关键字
            //    ref AnimationData animationData = ref blobBuilder.ConstructRoot<AnimationData>();

            //    animationData.frameMax = authoring.soldierIdle.meshArray.Length;
            //    animationData.frameTimerMax = authoring.soldierIdle.frameTimerMax;

            //    BlobBuilderArray<BatchMeshID> blobBuilderArray =
            //        blobBuilder.Allocate<BatchMeshID>(ref animationData.batchMeshIDBlobArray, authoring.soldierIdle.meshArray.Length);
            //    for (int i = 0; i < authoring.soldierIdle.meshArray.Length; i++)
            //    {
            //        Mesh mesh = authoring.soldierIdle.meshArray[i];
            //        blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
            //    }

            //    animationDataHolder.soldierIdle = blobBuilder.CreateBlobAssetReference<AnimationData>(Allocator.Persistent);

            //    //释放
            //    blobBuilder.Dispose();

            //    //将 Blob 资产注册到 Baker 中
            //    //out var hash 是该方法自动生成的去重哈希值，通常不需要手动处理它
            //    AddBlobAsset(ref animationDataHolder.soldierIdle, out Unity.Entities.Hash128 objectHash);
            //}
            #endregion

            //在自定义烘焙系统中再构建Blob资产
            #region blob
            /*
            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            //ref关键字
            ref BlobArray<AnimationData> animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();

            BlobBuilderArray<AnimationData> animationDataBlobBuilderArray
                = blobBuilder.Allocate(ref animationDataBlobArray, System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)).Length);
            */
            #endregion

            #region 硬编码
            //{
            //    animationDataBlobBuilderArray[0].frameMax = authoring.soldierIdle.meshArray.Length;
            //    animationDataBlobBuilderArray[0].frameTimerMax = authoring.soldierIdle.frameTimerMax;

            //    BlobBuilderArray<BatchMeshID> blobBuilderArray =
            //        blobBuilder.Allocate<BatchMeshID>(ref animationDataBlobBuilderArray[0].batchMeshIDBlobArray, authoring.soldierIdle.meshArray.Length);
            //    for (int i = 0; i < authoring.soldierIdle.meshArray.Length; i++)
            //    {
            //        Mesh mesh = authoring.soldierIdle.meshArray[i];
            //        blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
            //    }
            //}

            //{
            //    animationDataBlobBuilderArray[1].frameMax = authoring.soldierWalk.meshArray.Length;
            //    animationDataBlobBuilderArray[1].frameTimerMax = authoring.soldierWalk.frameTimerMax;

            //    BlobBuilderArray<BatchMeshID> blobBuilderArray =
            //        blobBuilder.Allocate<BatchMeshID>(ref animationDataBlobBuilderArray[1].batchMeshIDBlobArray, authoring.soldierWalk.meshArray.Length);
            //    for (int i = 0; i < authoring.soldierWalk.meshArray.Length; i++)
            //    {
            //        Mesh mesh = authoring.soldierWalk.meshArray[i];
            //        blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
            //    }
            //}
            #endregion

            int index = 0;
            //遍历动画类型
            foreach (AnimationDataSO.AnimationType animationType in System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)))
            {
                AnimationDataSO animationDataSO = authoring.animationDataListSO.GetAnimationDataSO(animationType);
                #region blob
                /*
                animationDataBlobBuilderArray[index].frameMax = animationDataSO.meshArray.Length;
                animationDataBlobBuilderArray[index].frameTimerMax = animationDataSO.frameTimerMax;

                BlobBuilderArray<BatchMeshID> blobBuilderArray =
                    blobBuilder.Allocate<BatchMeshID>(ref animationDataBlobBuilderArray[index].batchMeshIDBlobArray, animationDataSO.meshArray.Length);
                */
                #endregion

                //遍历网格
                for (int i = 0; i < animationDataSO.meshArray.Length; i++)
                {
                    Mesh mesh = animationDataSO.meshArray[i];

                    //创建额外实体 参数bakingOnlyEntity 仅在烘焙阶段存在
                    Entity additionalEntity = CreateAdditionalEntity(TransformUsageFlags.None, true);

                    //烘焙网格
                    AddComponent(additionalEntity, new MaterialMeshInfo());
                    AddComponent(additionalEntity, new RenderMeshUnmanaged()
                    {
                        materialForSubMesh = authoring.defaultMaterial,
                        mesh = mesh,
                    });
                    AddComponent(additionalEntity, new AnimationDataHolderSubEntity()
                    {
                        animationType = animationType,
                        meshIndex = i
                    });
                    /*
                    blobBuilderArray[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
                    */
                }

                index++;
            }

            #region blob
            /*
            animationDataHolder.animationDataBlobArrayBlobAssetReference =
                    blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);

            //释放
            blobBuilder.Dispose();

            //将 Blob 资产注册到 Baker 中
            //out var hash 是该方法自动生成的去重哈希值，通常不需要手动处理它
            AddBlobAsset(ref animationDataHolder.animationDataBlobArrayBlobAssetReference, out Unity.Entities.Hash128 objectHash);
            */
            #endregion

            AddComponent(entity, new AnimationDataHolderObjectData()
            {
                animationDataListSO = authoring.animationDataListSO,
            });

            AddComponent(entity, animationDataHolder);
        }
    }
}


public struct AnimationDataHolder : IComponentData
{
    //public BlobAssetReference<AnimationData> soldierIdle;
    //public BlobAssetReference<AnimationData> soldierWalk;

    public BlobAssetReference<BlobArray<AnimationData>> animationDataBlobArrayBlobAssetReference;
}

public struct AnimationData
{
    /// <summary>
    /// 总帧数
    /// </summary>
    public int frameMax;

    /// <summary>
    /// 计时器
    /// </summary>

    public float frameTimerMax;

    //public BlobArray<BatchMeshID> batchMeshIDBlobArray;
    public BlobArray<int> intMeshIDBlobArray;
}

public struct AnimationDataHolderSubEntity : IComponentData
{
    public AnimationDataSO.AnimationType animationType;
    public int meshIndex;
}

public struct AnimationDataHolderObjectData : IComponentData
{
    public UnityObjectRef<AnimationDataListSO> animationDataListSO;
}
