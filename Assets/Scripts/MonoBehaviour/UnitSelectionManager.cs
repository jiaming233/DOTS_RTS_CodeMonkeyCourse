using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            //for (int i = 0; i < unitMoverArray.Length; i++)
            //{
            //    //副本 未修改存储在实体中的数据
            //    UnitMover unitMover = unitMoverArray[i];
            //    unitMover.targetPosition = mouseWorldPosition;
            //    //更新实体的数据
            //    entityManager.SetComponentData(entityArray[i], unitMover);
            //}

            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                //副本 未修改存储在实体中的数据
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = mouseWorldPosition;
                unitMoverArray[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }
}
