using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    private ComponentLookup<LocalTransform> localTransformComponentLookup;
    private ComponentLookup<Health> healthComponentLookup;
    private ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        localTransformComponentLookup = state.GetComponentLookup<LocalTransform>();
        healthComponentLookup = state.GetComponentLookup<Health>(true);
        postTransformMatrixComponentLookup = state.GetComponentLookup<PostTransformMatrix>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        if (Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;// burst限制
        }

        localTransformComponentLookup.Update(ref state);
        healthComponentLookup.Update(ref state);
        postTransformMatrixComponentLookup.Update(ref state);

        new HeathBarJob()
        {
            localTransformComponentLookup = localTransformComponentLookup,
            healthComponentLookup = healthComponentLookup,
            postTransformMatrixComponentLookup = postTransformMatrixComponentLookup,
            cameraForward = cameraForward
        }.ScheduleParallel();

        #region 非Jobs
        //foreach ((
        //    RefRW<LocalTransform> localTransform,
        //    RefRO<HealthBar> healthBar) 
        //    in SystemAPI.Query<
        //        RefRW<LocalTransform>,
        //        RefRO<HealthBar>>())
        //{
        //    LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
        //    if(localTransform.ValueRO.Scale == 1f)
        //    {
        //        //将全局旋转转换为局部旋转
        //        localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
        //    }


        //    Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);

        //    if (!health.onHealthChange)
        //    {
        //        continue;
        //    }

        //    float healthNormalized = (float)health.healthAmount / health.healthAmountMax;

        //    if(healthNormalized == 1f)
        //    {
        //        localTransform.ValueRW.Scale = 0f;
        //    }
        //    else
        //    {
        //        //生命值不满时显示生命条
        //        localTransform.ValueRW.Scale = 1f;
        //    }

        //    RefRW<PostTransformMatrix> barVisualPostTransformMatrix = 
        //        SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
        //    barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
        //}
        #endregion
    }
}


[BurstCompile]
public partial struct HeathBarJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public ComponentLookup<Health> healthComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    public float3 cameraForward;

    /// <summary>
    /// 这里参数如果包含ref LocalTransform localTransform，和组件查找localTransformComponentLookup冲突
    /// </summary>
    /// <param name="healthBar"></param>
    /// <param name=""></param>
    public void Execute(/*ref LocalTransform localTransform, */in HealthBar healthBar, Entity entity)
    {
        RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);

        LocalTransform parentLocalTransform = localTransformComponentLookup[healthBar.healthEntity];

        if (localTransform.ValueRO.Scale == 1f)
        {
            //将全局旋转转换为局部旋转
            localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
        }


        Health health = healthComponentLookup[healthBar.healthEntity];

        if (!health.onHealthChange)
        {
            return;
        }

        float healthNormalized = (float)health.healthAmount / health.healthAmountMax;

        if (healthNormalized == 1f)
        {
            localTransform.ValueRW.Scale = 0f;
        }
        else
        {
            //生命值不满时显示生命条
            localTransform.ValueRW.Scale = 1f;
        }

        RefRW<PostTransformMatrix> barVisualPostTransformMatrix = postTransformMatrixComponentLookup.GetRefRW(healthBar.barVisualEntity);
        barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
    }
}
