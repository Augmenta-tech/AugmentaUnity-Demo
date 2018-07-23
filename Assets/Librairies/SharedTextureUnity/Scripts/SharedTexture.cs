using System.Collections;
using System.Collections.Generic;
using Spout;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SharedTexture : MonoBehaviour
{
    public Camera TargetCamera;
    public bool UseCameraResolution;
	public int outputWidth;
	public int outputHeight;

    private Camera _myCam;

    private int currentWidth, currentHeight;

    [Header("Spout settings")]
    public bool spoutOutput;
    public string sharingName = "UnitySender";

    private RenderTexture texture;
    private RenderTexture blackTex;

    private SpoutCamSender.TextureFormat textureFormat = SpoutCamSender.TextureFormat.DXGI_FORMAT_R8G8B8A8_UNORM;
    public bool debugConsole = false;

    public bool showTexture;
    public bool forceBlackTexture;


    [Header("Funnel settings")]
    [HideInInspector]
    /// Anti-aliasing (MSAA) option
    public int antiAliasing = 1;
    [HideInInspector]
    /// Discards alpha channel before sending
    public bool discardAlpha = true;
    [HideInInspector]
    /// Determines how to handle the rendered screen
    public Funnel.Funnel.RenderMode renderMode;


    private SpoutCamSender spout;
    private Funnel.Funnel funnel;
    private bool isSendingTexture; 

    public void NewTextureSize(int width, int height)
    {
        if(UseCameraResolution)
        {
            width = TargetCamera.pixelWidth;
            height = TargetCamera.pixelHeight;
			outputHeight = height;
			outputWidth = width;
        }

        if (width != currentWidth || height != currentHeight)
        {
            enabled = false;
            RenderTexture newText = new RenderTexture(width, height, 24);
			currentWidth = width;
			currentHeight = height;
			outputHeight = height;
			outputWidth = width;
			GetComponent<Camera>().targetTexture = newText;
            GetComponent<SpoutCamSender>().textureWidth = width;
            GetComponent<SpoutCamSender>().textureHeight = height;
            GetComponent<SpoutCamSender>().texture = newText;
            texture = newText;
            enabled = true;
        }
    }

    // Use this for initialization
    void Awake()
    {
        _myCam = GetComponent<Camera>();
        if (!this.isActiveAndEnabled) return;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        this.gameObject.SetActive(false); //to prevent awake method to execute before its variable initialization

        spout = gameObject.AddComponent<SpoutCamSender>();

        spout.sharingName = sharingName;
        spout.blackTex = blackTex;
        spout.texture = texture;
        spout.textureFormat = textureFormat;
        spout.debugConsole = debugConsole;
        spout.showTexture = showTexture;
        spout.forceBlackTexture = forceBlackTexture;

        NewTextureSize(outputWidth, outputHeight);

#endif
#if UNITY_EDITOR_MAC || UNITY_STANDALONE_OSX
        this.gameObject.SetActive(false);
        funnel = gameObject.AddComponent<Funnel.Funnel>(); //to prevent awake method to execute before its variable initialization

        funnel.antiAliasing = antiAliasing;
        funnel.discardAlpha = discardAlpha;
        funnel.renderMode = renderMode;
#endif
  
        this.gameObject.SetActive(true);
        isSendingTexture = true;
        this.enabled = spoutOutput;
    }

    void OnDisable()
    {
        if (spout != null)
            spout.enabled = false;

        if (funnel != null)
            funnel.enabled = false;
    }

    void OnEnable()
    {
        if (spout == null && funnel == null)
            Awake();

        if (spout != null)
            spout.enabled = true;

        if (funnel != null)
            funnel.enabled = true;
    }

    void UpdateCamera()
    {
        transform.position = TargetCamera.transform.position;
        transform.rotation = TargetCamera.transform.rotation;
        _myCam.farClipPlane = TargetCamera.farClipPlane;
        _myCam.nearClipPlane = TargetCamera.nearClipPlane;
        _myCam.orthographic = TargetCamera.orthographic;

        _myCam.cullingMask = TargetCamera.cullingMask;
        _myCam.backgroundColor = TargetCamera.backgroundColor;
        _myCam.clearFlags = TargetCamera.clearFlags;
        _myCam.renderingPath = TargetCamera.actualRenderingPath;
        _myCam.projectionMatrix = TargetCamera.projectionMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        if(!spoutOutput && isSendingTexture)
        {
            OnDisable();
            isSendingTexture = false;
        }
        if(spoutOutput && !isSendingTexture)
        {
            OnEnable();
            isSendingTexture = true;
        }

        UpdateCamera();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        if (spout == null) return;

        if (spout.sharingName != sharingName)
            spout.sharingName = sharingName;

        if (spout.blackTex != blackTex)
            spout.blackTex = blackTex;

        if (spout.texture != texture)
            spout.texture = texture;

        if (spout.textureFormat != textureFormat)
            spout.textureFormat = textureFormat;

        if (spout.debugConsole != debugConsole)
            spout.debugConsole = debugConsole;

        if (spout.showTexture != showTexture)
            spout.showTexture = showTexture;

        if (spout.forceBlackTexture != forceBlackTexture)
            spout.forceBlackTexture = forceBlackTexture;

#endif
#if UNITY_EDITOR_MAC || UNITY_STANDALONE_OSX

        if(funnel.antiAliasing != antiAliasing)
            funnel.antiAliasing = antiAliasing;

        if(funnel.discardAlpha != discardAlpha)
            funnel.discardAlpha = discardAlpha;

        if(funnel.renderMode != renderMode)
            funnel.renderMode = renderMode;
#endif
    }
}
