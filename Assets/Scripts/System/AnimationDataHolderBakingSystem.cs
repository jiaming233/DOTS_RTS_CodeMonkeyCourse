using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
[UpdateInGroup(typeof(PostBakingSystemGroup))]//在所有默认烘焙系统之后运行
partial struct AnimationDataHolderBakingSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //Debug.Log("AnimationDataHolderBakingSystem");

        AnimationDataListSO animationDataListSO = null;
        foreach (RefRO<AnimationDataHolderObjectData> animationDataHolderObjectData 
            in SystemAPI.Query<RefRO<AnimationDataHolderObjectData>>())
        {
            animationDataListSO = animationDataHolderObjectData.ValueRO.animationDataListSO;
        }

        Dictionary<AnimationDataSO.AnimationType, int[]> blobAssetDataDictionary
            = new Dictionary<AnimationDataSO.AnimationType, int[]>();

        foreach (AnimationDataSO.AnimationType animationType in System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)))
        {
            AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
            blobAssetDataDictionary[animationType] = new int[animationDataSO.meshArray.Length];
        }

        //遍历额外实体 填充字典
        foreach ((
            RefRO<AnimationDataHolderSubEntity> animationDataHolderSubEntity,
            RefRO<MaterialMeshInfo> materialMeshInfo)
            in SystemAPI.Query<
                RefRO<AnimationDataHolderSubEntity>,
                RefRO<MaterialMeshInfo>>())
        {

            ////使用Mesh而不是烘焙时的BatchMeshID
            //materialMeshInfo.ValueRO.MeshID
            blobAssetDataDictionary[animationDataHolderSubEntity.ValueRO.animationType][animationDataHolderSubEntity.ValueRO.meshIndex] 
                = materialMeshInfo.ValueRO.Mesh;

            //Debug.Log(animationDataHolderSubEntity.ValueRO.animationType +
            //    " :: " + animationDataHolderSubEntity.ValueRO.meshIndex +
            //    " = " + materialMeshInfo.ValueRO.Mesh);
        }

        //写入数据
        foreach (RefRW<AnimationDataHolder> animationDataHolder in SystemAPI.Query<RefRW<AnimationDataHolder>>())
        {
            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            ref BlobArray<AnimationData> animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();

            BlobBuilderArray<AnimationData> animationDataBlobBuilderArray
                = blobBuilder.Allocate(ref animationDataBlobArray, System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)).Length);


            int index = 0;
            //遍历动画类型
            foreach (AnimationDataSO.AnimationType animationType in System.Enum.GetValues(typeof(AnimationDataSO.AnimationType)))
            {
                AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);

                animationDataBlobBuilderArray[index].frameMax = animationDataSO.meshArray.Length;
                animationDataBlobBuilderArray[index].frameTimerMax = animationDataSO.frameTimerMax;

                BlobBuilderArray<int> blobBuilderArray =
                    blobBuilder.Allocate<int>(ref animationDataBlobBuilderArray[index].intMeshIDBlobArray, animationDataSO.meshArray.Length);
    
                //遍历网格
                for (int i = 0; i < animationDataSO.meshArray.Length; i++)
                {
                    blobBuilderArray[i] = blobAssetDataDictionary[animationType][i];
                }

                index++;
            }

            animationDataHolder.ValueRW.animationDataBlobArrayBlobAssetReference =
                    blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);

            //释放
            blobBuilder.Dispose();
        }
    }
}
