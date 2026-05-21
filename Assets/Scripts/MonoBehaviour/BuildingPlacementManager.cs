using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;


public class BuildingPlacementManager : MonoBehaviour
{
    public static BuildingPlacementManager Instance { get; private set; }

    public event EventHandler OnActiveBuildingTypeSOChanged;

    [SerializeField] private BuildingTypeSO buildingTypeSO;
    [SerializeField] private UnityEngine.Material ghostMaterial;

    private Transform ghostTransform;
    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(ghostTransform != null)
        {
            ghostTransform.position = MouseWorldPosition.Instance.GetPosition();
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (buildingTypeSO.IsNone())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            SetActiveBuildingTypeSO(GameAssets.Instance.buildingTypeListSO.none);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (CanPlaceBuilding())
            {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
                EntitiesReferences entitiesReferences = entityQuery.GetSingleton<EntitiesReferences>();

                Entity spawnedEntity = entityManager.Instantiate(buildingTypeSO.GetPrefabEntity(entitiesReferences));
                entityManager.SetComponentData(spawnedEntity, LocalTransform.FromPosition(MouseWorldPosition.Instance.GetPosition()));
            }
        }
    }

    private bool CanPlaceBuilding()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
        PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        CollisionFilter collisionFilter = new CollisionFilter()
        {
            BelongsTo = ~0u,
            CollidesWith = 1u << GameAssets.BUILDINGS_LAYER,
            GroupIndex = 0
        };

        Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();
        UnityEngine.BoxCollider boxCollider = buildingTypeSO.prefab.GetComponent<UnityEngine.BoxCollider>();
        float boundExtents = 1.1f;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);
        if (collisionWorld.OverlapBox(
            mousePosition,
            Quaternion.identity,
            boxCollider.size * 0.5f * boundExtents,
            ref distanceHitList,
            collisionFilter))
        {
            //hit building
            return false;
        }

        distanceHitList.Clear();
        if (collisionWorld.OverlapSphere(mousePosition, buildingTypeSO.buildingDistanceMin, ref distanceHitList, collisionFilter))
        {
            foreach (DistanceHit distanceHit in distanceHitList)
            {
                if (entityManager.HasComponent<BuildingTypeSOHolder>(distanceHit.Entity))
                {
                    BuildingTypeSOHolder buildingTypeSOHolder = entityManager.GetComponentData<BuildingTypeSOHolder>(distanceHit.Entity);
                    if (buildingTypeSOHolder.buildingType == buildingTypeSO.buildingType)
                    {
                        //same
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public BuildingTypeSO GetActiveBuildingTypeSO()
    {
        return buildingTypeSO;
    }

    public void SetActiveBuildingTypeSO(BuildingTypeSO buildingTypeSO)
    {
        this.buildingTypeSO = buildingTypeSO;

        if(ghostTransform  != null)
        {
            Destroy(ghostTransform.gameObject);
        }

        if (!buildingTypeSO.IsNone())
        {
            ghostTransform = Instantiate(buildingTypeSO.visualPrefab);
            foreach(MeshRenderer meshRenderer in ghostTransform.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = ghostMaterial;
            }
        }

        OnActiveBuildingTypeSOChanged?.Invoke(this, EventArgs.Empty);
    }
}
