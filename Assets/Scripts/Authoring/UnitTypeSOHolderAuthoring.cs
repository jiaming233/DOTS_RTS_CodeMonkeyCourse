using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class UnitTypeSOHolderAuthoring : MonoBehaviour
{
    public UnitTypeSO.UnitType unitType;

    public class Baker : Baker<UnitTypeSOHolderAuthoring>
    {
        public override void Bake(UnitTypeSOHolderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitTypeSOHolder()
            {
                unitType = authoring.unitType
            });
        }
    }
}

public struct UnitTypeSOHolder : IComponentData
{
    public UnitTypeSO.UnitType unitType;
}