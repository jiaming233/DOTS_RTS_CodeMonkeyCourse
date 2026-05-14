using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AnimationDataSO : ScriptableObject
{
    public enum AnimationType
    {
        None,
        SoldierIdle,
        SoldierWalk,
        ZombieIdle,
        ZombieWalk,
        SoldierShoot,
        SoldierAim,
        ZombieAttack
    }

    public AnimationType animationType;

    public Mesh[] meshArray;
    public float frameTimerMax;
}
