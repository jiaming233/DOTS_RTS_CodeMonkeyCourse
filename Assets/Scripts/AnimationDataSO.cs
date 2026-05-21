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
        ZombieAttack,
        ScoutIdle,
        ScoutWalk,
        ScoutShoot,
        ScoutAim,
    }

    public AnimationType animationType;

    public Mesh[] meshArray;
    public float frameTimerMax;

    public static bool IsAnimationUninterruptable(AnimationType animationType)
    {
        switch (animationType)
        {
            default:
                return false;
            case AnimationType.SoldierShoot:
            case AnimationType.ZombieAttack:
            case AnimationType.ScoutShoot:
                return true;
        }
    }
}
