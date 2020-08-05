using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using System.Linq;

public class RipplesComputeController : MonoBehaviour
{
    [Header("Augmenta")]
    public Augmenta.AugmentaManager augmentaManager;

    [Header("Raycast")]
    public LayerMask textureLayerMask;
    public Camera cam;

    [Header("Compute Shader")]
    public ComputeShader ripplesCompute;
    public int numThreadsPerGroup = 8;

    public Vector2Int textureSize = new Vector2Int(2048, 2048);

    public Material ripplesMaterial;

    [Header("Ripples Parameters")]
    public int updatePerSeconds = 60;
    public Vector2 ripplesSize = Vector2.one;
    public float ripplesScale = 5.0f;
    public float ripplesAttenuation = 0.01f;

    private RenderTexture _ripplesTexture0;
    private RenderTexture _ripplesTexture1;
    private int _ripplesInitKernel;
    private int _ripplesUpdateKernel;

    private RaycastHit _raycastHit;
    private Ray _ray;

    private bool _readWriteFlag = true;

    [Header("Debug")]
    public Vector4[] _augmentaPoints;
    private ComputeBuffer _augmentaPointsBuffer;
    private int _maxAugmentaPointsCount = 20;

    private float _updateTimer = 0;

    private void OnEnable() {

        //Get kernel
        _ripplesInitKernel = ripplesCompute.FindKernel("InitRipples");
        _ripplesUpdateKernel = ripplesCompute.FindKernel("UpdateRipples");

        CreateRipplesTextures();
        InitRipples();

        //Allocate buffers for maxAugmentaPointsCount
        _augmentaPoints = new Vector4[_maxAugmentaPointsCount];
        for (int i = 0; i < _maxAugmentaPointsCount; i++)
            _augmentaPoints[i] = Vector4.zero;

        _augmentaPointsBuffer = new ComputeBuffer(_maxAugmentaPointsCount, sizeof(float) * 4);
        _augmentaPointsBuffer.SetData(_augmentaPoints);
    }

    // Update is called once per frame
    void Update()
    {

        GetMousePosition();
        UpdateAugmentaPositions();

        //Update the ripples at desired framerate
        _updateTimer += Time.deltaTime;
        if(_updateTimer > 1.0f / updatePerSeconds) {
            ComputeRipples();
            _updateTimer = 0;
		}

        BindRipplesTextureToMaterial();
    }

	private void OnDisable() {

        _augmentaPointsBuffer.Release();
    }

    void CreateRipplesTextures() {

        //Create ripple textures
        _ripplesTexture0 = new RenderTexture(textureSize.x, textureSize.y, 1, RenderTextureFormat.ARGBFloat);
        _ripplesTexture0.enableRandomWrite = true;
        _ripplesTexture0.Create();

        _ripplesTexture1 = new RenderTexture(textureSize.x, textureSize.y, 1, RenderTextureFormat.ARGBFloat);
        _ripplesTexture1.enableRandomWrite = true;
        _ripplesTexture1.Create();
    }

    void GetMousePosition() {

        //Raycast screen mouse position to the ripple floor object
        _ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, textureLayerMask)) {
            ripplesCompute.SetVector("_Mouse", new Vector4(_raycastHit.textureCoord.x, _raycastHit.textureCoord.y, 0, 1));
        } else {
            ripplesCompute.SetVector("_Mouse", new Vector4(0, 0, 0, 0));
        }
    }

    void UpdateAugmentaPositions() {

        int augmentaPointsCount = Mathf.Min(augmentaManager.augmentaObjects.Count, _maxAugmentaPointsCount);

        for(int i=0; i<augmentaPointsCount; i++) {

            if (augmentaManager.augmentaObjects.ElementAt(i).Value.ageInSeconds > 0) {  //Wait until point is ready to get its information

                //Raycast from camera to augmenta point to get the point position on the ripples texture
                _ray = new Ray(cam.transform.position, augmentaManager.augmentaObjects.ElementAt(i).Value.worldPosition2D - cam.transform.position);

                if (Physics.Raycast(_ray, out _raycastHit, Mathf.Infinity, textureLayerMask)) {
                    _augmentaPoints[i] = new Vector4(_raycastHit.textureCoord.x, _raycastHit.textureCoord.y, 0, 1);
                } else {
                    _augmentaPoints[i] = Vector4.zero;
                }

            }
        }

        //Clear data for unused points
        for(int i=augmentaPointsCount; i<_maxAugmentaPointsCount; i++)
            _augmentaPoints[i] = Vector4.zero;

        //Send data to the compute shader
        _augmentaPointsBuffer.SetData(_augmentaPoints);
        ripplesCompute.SetBuffer(_ripplesUpdateKernel, "_AugmentaPoints", _augmentaPointsBuffer);
        ripplesCompute.SetInt("_AugmentaPointsCount", augmentaPointsCount);
    }

    void InitRipples() {

        ripplesCompute.SetTexture(_ripplesInitKernel, "_RipplesTextureRead", _readWriteFlag ? _ripplesTexture0 : _ripplesTexture1);
        ripplesCompute.SetTexture(_ripplesInitKernel, "_RipplesTextureWrite", _readWriteFlag ? _ripplesTexture1 : _ripplesTexture0);

        ripplesCompute.Dispatch(_ripplesInitKernel, textureSize.x / numThreadsPerGroup, textureSize.y / numThreadsPerGroup, 1);
    }

    void ComputeRipples() {

        _readWriteFlag = !_readWriteFlag;

        ripplesCompute.SetFloat("_RipplesWidth", ripplesSize.x);
        ripplesCompute.SetFloat("_RipplesHeight", ripplesSize.y);
        ripplesCompute.SetFloat("_RipplesScale", ripplesScale);
        ripplesCompute.SetFloat("_RipplesAttenuation", ripplesAttenuation);
        ripplesCompute.SetFloat("_TextureWidth", textureSize.x);
        ripplesCompute.SetFloat("_TextureHeight", textureSize.y);
        ripplesCompute.SetTexture(_ripplesUpdateKernel, "_RipplesTextureRead", _readWriteFlag ? _ripplesTexture0 : _ripplesTexture1);
        ripplesCompute.SetTexture(_ripplesUpdateKernel, "_RipplesTextureWrite", _readWriteFlag ? _ripplesTexture1 : _ripplesTexture0);

        ripplesCompute.Dispatch(_ripplesUpdateKernel, textureSize.x / numThreadsPerGroup, textureSize.y / numThreadsPerGroup, 1);
    }

    void BindRipplesTextureToMaterial() {

        //Bind ripple texture to the ripple material
        ripplesMaterial.SetTexture("_RipplesTex", _readWriteFlag ? _ripplesTexture1 : _ripplesTexture0);
        ripplesMaterial.SetVector("_TextureSize", new Vector4(textureSize.x, textureSize.y, 0, 0));
    }
}
