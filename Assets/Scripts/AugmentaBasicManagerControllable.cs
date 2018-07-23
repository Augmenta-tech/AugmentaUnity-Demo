using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaBasicManagerControllable : Controllable {

    public AugmentaBasicManager augmentaBasicManager;

    [OSCProperty]
    [Range(1, 20)]
    public float PositionFollowTightness;

    [OSCProperty]
    [Range(1, 20)]
    public int VelocityAverageValueCount;

    public override void Awake()
    {
        if (augmentaBasicManager == null)
            augmentaBasicManager = FindObjectOfType<AugmentaBasicManager>();

        if (augmentaBasicManager == null)
        {
            Debug.LogWarning("Can't find " + this.GetType().Name + " script to control !");
            return;
        }

        TargetScript = augmentaBasicManager;
        base.Awake();
    }
}
