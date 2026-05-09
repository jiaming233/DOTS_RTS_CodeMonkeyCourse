using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class LoseTargetAuthoring : MonoBehaviour
{
    public float lostTargetDistance;

    public class Baker : Baker<LoseTargetAuthoring>
    {
        public override void Bake(LoseTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LoseTarget()
            {
                lostTargetDistance = authoring.lostTargetDistance,
            });
        }
    }
}

public struct LoseTarget : IComponentData
{
    public float lostTargetDistance;
}