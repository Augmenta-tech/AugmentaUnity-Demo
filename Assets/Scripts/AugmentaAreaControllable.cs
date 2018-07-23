using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaAreaControllable : Controllable
{
    [Header("Global Augmenta settings")]

    [OSCProperty(isInteractible = false)]
    public int NbAugmentaPoints;

    [OSCProperty]
    public float PixelPerMeter;

    [Header("Points")]
    [OSCProperty(TargetList = "Modes")]
    public string AugmentaMode;

    [OSCProperty]
    public int AskedPoints;

    [Header("Debug")]
    [OSCProperty]
    public bool Mire;

    [OSCProperty]
    public bool AugmentaDebug;

    [OSCProperty]
    [Range(0.0f, 1.0f)]
    public float DebugTransparency;

    public List<string> Modes;

    public AugmentaArea MyAugmentaArea;

    public override void Awake()
    {
        if (MyAugmentaArea == null)
            MyAugmentaArea = FindObjectOfType<AugmentaArea>();

        if (MyAugmentaArea == null)
        {
            Debug.LogWarning("Can't find " + this.GetType().Name + " script to control !");
            return;
        }

        AugmentaMode = MyAugmentaArea.ActualPointType.ToString();
        Modes.Add(AugmentaPointType.AllPoints.ToString());
        Modes.Add(AugmentaPointType.Oldest.ToString());
        Modes.Add(AugmentaPointType.Newest.ToString());
 
        DebugTransparency = 1.0f;
        TargetScript = MyAugmentaArea;
        usePresets = false;
        base.Awake();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        MyAugmentaArea.ActualPointType = (AugmentaPointType)Enum.Parse(typeof(AugmentaPointType), AugmentaMode);
    }

    public override void OnScriptValueChanged(string name)
    {
        base.OnScriptValueChanged(name);
        AugmentaMode = MyAugmentaArea.ActualPointType.ToString();
    }
}
