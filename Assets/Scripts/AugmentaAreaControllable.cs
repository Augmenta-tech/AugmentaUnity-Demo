﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaAreaControllable : Controllable
{
    [Header("Global Augmenta settings")]

    [OSCProperty]
    public bool cameraRendering;

    [OSCProperty]
    public int InputPort;

    [OSCProperty(isInteractible = false)]
    public bool connected;

    [OSCProperty(TargetList = "versions")]
    public string version;

    [OSCProperty][Tooltip("for protocol V1 only.")]
    public float meterPerPixel;

	[OSCProperty]
	public float scaling;

	[OSCProperty]
    public bool FlipX;

    [OSCProperty]
    public bool FlipY;

    [Header("Points")]

    [OSCProperty(isInteractible = false)]
    public int NbAugmentaPeople;

    [OSCProperty(TargetList = "Modes")]
    public string AugmentaMode;

    [OSCProperty]
    public int AskedPeople;

    [Header("Debug")]

    [OSCProperty]
    public bool Mire;

    [OSCProperty]
    public bool AugmentaDebug;

    [OSCProperty]
    [Range(0.0f, 1.0f)]
    public float DebugTransparency;

    public List<string> Modes;
    public List<string> versions;

    public override void Awake()
    {
        AugmentaMode = ((AugmentaArea)TargetScript).ActualPersonType.ToString();
        Modes.Add(AugmentaPersonType.AllPeople.ToString());
        Modes.Add(AugmentaPersonType.Oldest.ToString());
        Modes.Add(AugmentaPersonType.Newest.ToString());

        version = ((AugmentaArea)TargetScript).protocolVersion.ToString();
        versions.Add(ProtocolVersion.v1.ToString());
        versions.Add(ProtocolVersion.v2.ToString());
 
        DebugTransparency = 1.0f;
        base.Awake();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        ((AugmentaArea)TargetScript).ActualPersonType = (AugmentaPersonType)Enum.Parse(typeof(AugmentaPersonType), AugmentaMode);
        ((AugmentaArea)TargetScript).protocolVersion = (ProtocolVersion)Enum.Parse(typeof(ProtocolVersion), version);
    }

    public override void OnScriptValueChanged(string name)
    {
        base.OnScriptValueChanged(name);
        AugmentaMode = ((AugmentaArea)TargetScript).ActualPersonType.ToString();
        version = ((AugmentaArea)TargetScript).protocolVersion.ToString();
    }
}
