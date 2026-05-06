using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private RectTransform selectionAreaRectTransform;

    private void Start()
    {
        selectionAreaRectTransform.gameObject.SetActive(false);

        UnitSelectionManager.Instance.OnSelectionAreaStart += UnitSelectionManager_OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += UnitSelectionManager_OnSelectionAreaEnd;
    }


    private void UnitSelectionManager_OnSelectionAreaStart(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(true);
        UpdateVisual();
    }


    private void UnitSelectionManager_OnSelectionAreaEnd(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }

    public void UpdateVisual()
    {
        Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();

        float canvasScale = canvas.transform.localScale.x;

        selectionAreaRectTransform.anchoredPosition = selectionAreaRect.position / canvasScale;

        selectionAreaRectTransform.sizeDelta = selectionAreaRect.size / canvasScale;
    }
}
