using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPositionUI : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
