using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public const int UNITS_LAYER = 6;

    public static GameAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


}
