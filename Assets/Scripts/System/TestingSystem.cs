using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct TestingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        int unitCount = 0;

        //foreach ((
        //    RefRW<LocalTransform> localTransform,
        //    RefRW<UnitMover> unitMover,
        //    RefRW<PhysicsVelocity> physicsVelocity,
        //    RefRO<Selected> selected)
        //    in SystemAPI.Query<
        //        RefRW<LocalTransform>,
        //        RefRW<UnitMover>,
        //        RefRW<PhysicsVelocity>,
        //        RefRO<Selected>>())
        //{
        //    unitCount++;
        //}

        //foreach ((
        //  RefRW<LocalTransform> localTransform,
        //  RefRW<UnitMover> unitMover,
        //  RefRW<PhysicsVelocity> physicsVelocity)
        //  in SystemAPI.Query<
        //      RefRW<LocalTransform>,
        //      RefRW<UnitMover>,
        //      RefRW<PhysicsVelocity>>().WithDisabled<Selected>())
        //{
        //    unitCount++;
        //}

        //通过阵营查询
        foreach (RefRW<Friendly> friendly in SystemAPI.Query<RefRW<Friendly>>())
        {
            unitCount++;
        } 

        //Debug.Log($"unitCount:{unitCount}");
    }
}
