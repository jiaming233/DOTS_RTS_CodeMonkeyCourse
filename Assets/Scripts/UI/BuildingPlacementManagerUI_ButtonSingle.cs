using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacementManagerUI_ButtonSingle : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image selectedImage;
    [SerializeField] private Image iconImage;

    private BuildingTypeSO buildingTypeSO;

    public void Setup(BuildingTypeSO buildingTypeSO)
    {
        this.buildingTypeSO = buildingTypeSO;

        iconImage.sprite = buildingTypeSO.sprite;

        button.onClick.AddListener(() =>
        {
            BuildingPlacementManager.Instance.SetActiveBuildingTypeSO(buildingTypeSO);
        });
    }

    public void ShowSelected()
    {
        selectedImage.enabled = true;
    }

    public void HideSelected()
    {
        selectedImage.enabled = false;
    }
}
