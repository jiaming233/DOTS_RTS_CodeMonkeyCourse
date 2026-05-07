using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

partial struct HealthDeadTestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //新建实体命令缓冲区
        //EntityCommandBuffer entityCommandBuffer =  new EntityCommandBuffer(Allocator.Temp);
        //使用预设实体命令缓冲区
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach((
            RefRO<Health> health, 
            Entity entity)//WithEntityAccess 实体作为最后一个参数
            in SystemAPI.Query<
                RefRO<Health>>().WithEntityAccess())
        {
            if(health.ValueRO.healthAmount <= 0)
            {
                //entity is dead

                ////迭代时 不能销毁实体，是结构更改操作（DOTS整理内存）
                //state.EntityManager.DestroyEntity(entity);

                //使用实体命令缓冲
                entityCommandBuffer.DestroyEntity(entity);
            }
        }

        ////预设实体命令缓冲区不需要手动调用，会在帧结束时自动执行
        //entityCommandBuffer.Playback(state.EntityManager);
    }
}
