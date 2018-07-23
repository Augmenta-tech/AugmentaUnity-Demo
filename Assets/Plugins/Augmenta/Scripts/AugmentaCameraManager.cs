using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaCameraManager : MonoBehaviour
{
    public float CamDistToAugmenta;
    public float NearFrustrum = 0.01f;
    public bool drawNearCone, drawFrustum;

    public static AugmentaCameraManager Instance;

    private Vector3 BottomLeftCorner;
    private Vector3 BottomRightCorner;
    private Vector3 TopLeftCorner;
    private Vector3 TopRightCorner;
    public Transform lookTarget;

    public delegate void CameraUpdated(AugmentaSceneSettings settings);
    public static event CameraUpdated cameraUpdated;

    private static Camera theCam;

    void Awake()
    {
        theCam = GetComponent<Camera>();
        Instance = this;
    }

    public void UpdateCameraSettings(AugmentaSceneSettings augmentaSceneSettings)
    {
       // Debug.Log("Pixel meter coeff : " + augmentaSceneSettings.PixelToMeterCoeff);
        AugmentaArea.Instance.Zoom = augmentaSceneSettings.Zoom;
        AugmentaArea.Instance.PointTimeOut = augmentaSceneSettings.PointTimeOut;

        theCam.renderingPath = augmentaSceneSettings.MyCameraRenderingPath;
        theCam.backgroundColor = augmentaSceneSettings.BackgroundColor;
        theCam.orthographic = augmentaSceneSettings.UseOrtho;
        theCam.farClipPlane = augmentaSceneSettings.Far;
        theCam.nearClipPlane = augmentaSceneSettings.Near;
        theCam.clearFlags = augmentaSceneSettings.MyCameraClearFlags;
        CamDistToAugmenta = Mathf.Clamp(augmentaSceneSettings.CamDistToAugmenta, 1.0f, 500);

        augmentaSceneSettings.gameObject.GetComponent<Camera>().enabled = false;

        if (cameraUpdated != null)
            cameraUpdated(augmentaSceneSettings);
    }

    /** The width of the screen */
    public float Width
    {
        get
        {
            return (BottomRightCorner - BottomLeftCorner).magnitude;
        }
        set
        {
            Vector3 vecWidth = BottomRightCorner - BottomLeftCorner;
            float scale = value / vecWidth.magnitude;
            vecWidth *= (1 - scale);

            TopLeftCorner += vecWidth / 2;
            BottomLeftCorner += vecWidth / 2;
            BottomRightCorner -= vecWidth / 2;
        }
    }

    /** The height of the screen */
    public float Height
    {
        get
        {
            return (TopLeftCorner - BottomLeftCorner).magnitude;
        }
        set
        {
            Vector3 vecHeight = TopLeftCorner - BottomLeftCorner;
            float scale = value / vecHeight.magnitude;
            vecHeight *= (1 - scale);

            TopLeftCorner -= vecHeight / 2;
            BottomLeftCorner += vecHeight / 2;
            BottomRightCorner += vecHeight / 2;
        }
    }

    void Update()
    {
        BottomLeftCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0));
        BottomRightCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0));
        TopLeftCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0));
        TopRightCorner = AugmentaArea.Instance.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0));

        theCam.transform.localPosition = new Vector3(0.0f, 0.0f, CamDistToAugmenta);

        if (theCam.orthographic)
        {
            //transform.localRotation = Quaternion.Euler(new Vector3(0, 1, 1) * -180);
            ComputeOrthoCamera();
        }
        else
        {
            //transform.localRotation = Quaternion.Euler(new Vector3(0,180,0));
            ComputeOffCenterCamera();
        }

    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(BottomLeftCorner, 0.25f);
    //    Gizmos.DrawSphere(TopRightCorner, 0.25f);
    //}

    void ComputeOrthoCamera()
    {
        theCam.aspect = AugmentaArea.Instance.AspectRatio;
        theCam.orthographicSize = AugmentaArea.Instance.transform.localScale.y / 2;
        theCam.ResetProjectionMatrix();
    }

    void ComputeOffCenterCamera()
    {
        theCam.ResetAspect();

        Vector3 pa, pb, pc, pd;
        pa = BottomLeftCorner; //Bottom-Left
        pb = BottomRightCorner; //Bottom-Right
        pc = TopLeftCorner; //Top-Left
        pd = TopRightCorner; //Top-Right

        Vector3 pe = theCam.transform.position;// eye position

        Vector3 vr = (pb - pa).normalized; // right axis of screen
        Vector3 vu = (pc - pa).normalized; // up axis of screen
        Vector3 vn = Vector3.Cross(vr, vu).normalized; // normal vector of screen

        Vector3 va = pa - pe; // from pe to pa
        Vector3 vb = pb - pe; // from pe to pb
        Vector3 vc = pc - pe; // from pe to pc
        Vector3 vd = pd - pe; // from pe to pd

        float n = lookTarget.InverseTransformPoint(theCam.transform.position).z; // distance to the near clip plane (screen)
        float f = theCam.farClipPlane; // distance of far clipping plane
        float d = Vector3.Dot(va, vn); // distance from eye to screen
        float l = Vector3.Dot(vr, va) * n / d; // distance to left screen edge from the 'center'
        float r = Vector3.Dot(vr, vb) * n / d; // distance to right screen edge from 'center'
        float b = Vector3.Dot(vu, va) * n / d; // distance to bottom screen edge from 'center'
        float t = Vector3.Dot(vu, vc) * n / d; // distance to top screen edge from 'center'

        Matrix4x4 p = new Matrix4x4(); // Projection matrix
        p[0, 0] = 2.0f * n / (r - l);
        p[0, 2] = (r + l) / (r - l);
        p[1, 1] = 2.0f * n / (t - b);
        p[1, 2] = (t + b) / (t - b);
        p[2, 2] = (f + n) / (n - f);
        p[2, 3] = 2.0f * f * n / (n - f) * NearFrustrum;
        p[3, 2] = -1.0f;

        try
        {
            theCam.projectionMatrix = p; // Assign matrix to camera
        }
        catch (Exception e)
        {
            Debug.LogWarning("Frustrum error, matrix invalid : " + e.Message);
        }

        if (drawNearCone)
        { //Draw lines from the camera to the corners f the screen
            Debug.DrawRay(theCam.transform.position, va, Color.blue);
            Debug.DrawRay(theCam.transform.position, vb, Color.blue);
            Debug.DrawRay(theCam.transform.position, vc, Color.blue);
            Debug.DrawRay(theCam.transform.position, vd, Color.blue);
        }

        if (drawFrustum) DrawFrustum(theCam); //Draw actual camera frustum
    }

    Vector3 ThreePlaneIntersection(Plane p1, Plane p2, Plane p3)
    { //get the intersection point of 3 planes
        return ((-p1.distance * Vector3.Cross(p2.normal, p3.normal)) +
                (-p2.distance * Vector3.Cross(p3.normal, p1.normal)) +
                (-p3.distance * Vector3.Cross(p1.normal, p2.normal))) /
            (Vector3.Dot(p1.normal, Vector3.Cross(p2.normal, p3.normal)));
    }

    void DrawFrustum(Camera cam)
    {
        Vector3[] nearCorners = new Vector3[4]; //Approx'd nearplane corners
        Vector3[] farCorners = new Vector3[4]; //Approx'd farplane corners
        Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(cam); //get planes from matrix
        Plane temp = camPlanes[1]; camPlanes[1] = camPlanes[2]; camPlanes[2] = temp; //swap [1] and [2] so the order is better for the loop

        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = ThreePlaneIntersection(camPlanes[4], camPlanes[i], camPlanes[(i + 1) % 4]); //near corners on the created projection matrix
            farCorners[i] = ThreePlaneIntersection(camPlanes[5], camPlanes[i], camPlanes[(i + 1) % 4]); //far corners on the created projection matrix
        }

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4], Color.red, Time.deltaTime, false); //near corners on the created projection matrix
            Debug.DrawLine(farCorners[i], farCorners[(i + 1) % 4], Color.red, Time.deltaTime, false); //far corners on the created projection matrix
            Debug.DrawLine(nearCorners[i], farCorners[i], Color.red, Time.deltaTime, false); //sides of the created projection matrix
        }
    }
}
