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
    public float MeterPerPixel;

    [OSCProperty]
    public bool FlipX;

    [OSCProperty]
    public bool FlipY;

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

        AugmentaMode = MyAugmentaArea.ActualPersonType.ToString();
        Modes.Add(AugmentaPersonType.AllPeople.ToString());
        Modes.Add(AugmentaPersonType.Oldest.ToString());
        Modes.Add(AugmentaPersonType.Newest.ToString());
 
        DebugTransparency = 1.0f;
        TargetScript = MyAugmentaArea;
        base.Awake();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        MyAugmentaArea.ActualPersonType = (AugmentaPersonType)Enum.Parse(typeof(AugmentaPersonType), AugmentaMode);
    }

    public override void OnScriptValueChanged(string name)
    {
        base.OnScriptValueChanged(name);
        AugmentaMode = MyAugmentaArea.ActualPersonType.ToString();
    }
}
