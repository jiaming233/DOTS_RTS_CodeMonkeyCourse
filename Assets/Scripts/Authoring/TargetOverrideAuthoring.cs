using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TargetOverrideAuthoring : MonoBehaviour
{
    public class Baker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TargetOverride());
        }
    }
}

public struct TargetOverride : IComponentData
{
    public Entity targetEntity;
}