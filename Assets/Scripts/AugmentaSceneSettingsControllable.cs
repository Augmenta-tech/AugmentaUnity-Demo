using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaSceneSettingsControllable : Controllable {

    [Header("Augmenta settings")]
    [OSCProperty]
    public float Zoom;
    [OSCProperty]
    public float PointTimeOut;
    [Header("Camera settings")]
    [OSCProperty]
    public bool UseOrtho;
    [OSCProperty]
    [Range(0.01f, 10000f)]
    public float Far;
    [OSCProperty]
    [Range(0.01f, 10f)]
    public float Near;
    [Range(0.01f, 500f)]
    [OSCProperty]
    public float CamDistToAugmenta;

    public AugmentaSceneSettings MyAugmentaSceneSettings;

    public override void Awake()
    {
        MyAugmentaSceneSettings = FindObjectOfType<AugmentaSceneSettings>();
        if (MyAugmentaSceneSettings == null)
            Debug.LogWarning("Couldn't find a " + this.GetType().Name + " script");
        TargetScript = MyAugmentaSceneSettings;
        base.Awake();
    }

    public override void DataLoaded()
    {
        base.DataLoaded();
        MyAugmentaSceneSettings.UpdateCoreCamera();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        MyAugmentaSceneSettings.UpdateCoreCamera();
    }
}
