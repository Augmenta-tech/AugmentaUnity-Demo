using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class AugmentaSceneSettings : MonoBehaviour {

    [Header("Augmenta settings")]
    public float Zoom = 1;
    [Tooltip("In seconds")]
    public float PointTimeOut = 1;

    [Header("Camera settings")]
    public CameraClearFlags MyCameraClearFlags;
    public RenderingPath MyCameraRenderingPath;
    public Color BackgroundColor;
    public bool UseOrtho;

    [Range(0.01f, 10000f)]
    public float Far;
    [Range(0.01f, 10f)]
    public float Near;

    [Header("OffAxis settings")]
    [Range(0.01f,500f)]
    public float CamDistToAugmenta;

    // Use this for initialization
    void Start () {
        UpdateCoreCamera();
    }

    void Update()
    {
        UpdateCoreCamera();
    }

    public void UpdateCoreCamera()
    {
        if(AugmentaCameraManager.Instance != null)
            AugmentaCameraManager.Instance.UpdateCameraSettings(this);
    }
}
