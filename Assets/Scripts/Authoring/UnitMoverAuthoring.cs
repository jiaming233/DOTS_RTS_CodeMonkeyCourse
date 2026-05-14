using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;

    public class Baker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover()
            {
                //使用创作类的value进行初始化
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed
            });
        }
    }
}

/// <summary>
/// 单位移动器组件
/// </summary>
public struct UnitMover : IComponentData
{
    public float moveSpeed;
    public float rotationSpeed;
    public float3 targetPosition;

    public bool IsMoving;
}