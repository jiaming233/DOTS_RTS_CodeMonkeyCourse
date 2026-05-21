using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform buildingContainer;
    [SerializeField] private RectTransform buildingButtonTemplate;
    [SerializeField] private BuildingTypeListSO buildingTypeListSO;

    private Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_ButtonSingle> buildingButtonDictionary;

    private void Awake()
    {
        buildingButtonTemplate.gameObject.SetActive(false);

        buildingButtonDictionary = new Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_ButtonSingle>();

        foreach (BuildingTypeSO buildingTypeSO in buildingTypeListSO.buildingTypeSOList)
        {
            if (!buildingTypeSO.showInBuildingPlacementManagerUI)
            {
                continue;
            }

            RectTransform buildingRectTransform = Instantiate(buildingButtonTemplate, buildingContainer);
            buildingRectTransform.gameObject.SetActive(true);

            BuildingPlacementManagerUI_ButtonSingle buttonSingle = buildingRectTransform.GetComponent<BuildingPlacementManagerUI_ButtonSingle>();
            buttonSingle.Setup(buildingTypeSO);

            buildingButtonDictionary.Add(buildingTypeSO, buttonSingle);
        }
    }

    private void Start()
    {
        BuildingPlacementManager.Instance.OnActiveBuildingTypeSOChanged += BuildingPlacementManager_OnActiveBuildingTypeSOChanged;
        UpdateSelectedVisual();
    }

    private void BuildingPlacementManager_OnActiveBuildingTypeSOChanged(object sender, System.EventArgs e)
    {
        UpdateSelectedVisual();
    }

    private void UpdateSelectedVisual()
    {
        foreach(BuildingTypeSO buildingTypeSO in buildingButtonDictionary.Keys)
        {
            buildingButtonDictionary[buildingTypeSO].HideSelected();
        }
        buildingButtonDictionary[BuildingPlacementManager.Instance.GetActiveBuildingTypeSO()].ShowSelected();
    }
}
