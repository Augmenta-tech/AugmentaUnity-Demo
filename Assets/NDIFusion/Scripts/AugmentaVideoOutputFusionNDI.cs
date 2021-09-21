using Augmenta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaVideoOutputFusionNDI : MonoBehaviour
{
	[Header("Augmenta")]
	public AugmentaVideoOutput augmentaVideoOutput;

	[Header("NDI")]
	public Klak.Ndi.NdiReceiver ndiReceiver;
	public Renderer ndiRenderer;

	public bool showFusionNdi = false;
	public bool autoFindFusionNdi = true;
	public string fusionNdiName = "Augmenta Fusion - Scene";

	private RenderTexture _ndiTexture;
	private GameObject _ndiObject;

	private bool _initialized = false;

	private void OnEnable() {

		InitializeNdi();
	}

	private void Update() {

		if (showFusionNdi && !_ndiObject.activeSelf) {
			_ndiObject.SetActive(true);
		} else if (!showFusionNdi && _ndiObject.activeSelf) {
			_ndiObject.SetActive(false);
		}


		if (showFusionNdi) {
			if (!_initialized || augmentaVideoOutput.videoOutputSizeInPixels.x != _ndiTexture.width || augmentaVideoOutput.videoOutputSizeInPixels.y != _ndiTexture.height)
				InitializeNdi();

			if (ndiReceiver.ndiName != fusionNdiName)
				InitializeNdi();

			UpdateNdiObject();
		}
	}

	private void OnDisable() {

		DisableNdi();
	}

	void InitializeNdi() {

		//Get ndi object
		_ndiObject = ndiRenderer.gameObject;

		bool foundFusionNdi = false;

		//Set ndi source name
		if (autoFindFusionNdi) {
			foreach (var source in Klak.Ndi.NdiFinder.sourceNames) {
				if (source.Contains("Augmenta Fusion")) {
					ndiReceiver.ndiName = source;
					fusionNdiName = ndiReceiver.ndiName;
					foundFusionNdi = true;
					break;
				}
			}

			if (!foundFusionNdi)
				return;
		} else {
			ndiReceiver.ndiName = fusionNdiName;
		}

		//If video output has 0 size, abort
		if (augmentaVideoOutput.videoOutputSizeInPixels.x == 0 || augmentaVideoOutput.videoOutputSizeInPixels.y == 0)
			return;

		//Create ndi texture
		_ndiTexture = new RenderTexture(augmentaVideoOutput.videoOutputSizeInPixels.x, augmentaVideoOutput.videoOutputSizeInPixels.y, 0, RenderTextureFormat.ARGB32);
		//Assign texture to ndi receiver
		ndiReceiver.targetTexture = _ndiTexture;
		//Assign texture to ndi display
		ndiRenderer.material.SetTexture("_MainTex", _ndiTexture);

		_initialized = true;
	}

	void DisableNdi() {

		if (_ndiTexture)
			_ndiTexture.Release();
	}

	void UpdateNdiObject() {

		//Place ndi display
		transform.position = augmentaVideoOutput.botLeftCorner + 0.5f * (augmentaVideoOutput.topLeftCorner - augmentaVideoOutput.botLeftCorner) + 0.5f * (augmentaVideoOutput.topRightCorner - augmentaVideoOutput.topLeftCorner);
		transform.localScale = new Vector3(Vector3.Distance(augmentaVideoOutput.botRightCorner, augmentaVideoOutput.botLeftCorner), Vector3.Distance(augmentaVideoOutput.topLeftCorner, augmentaVideoOutput.botLeftCorner), 1);

		Vector3 _videoOutputUp = (augmentaVideoOutput.topLeftCorner - augmentaVideoOutput.botLeftCorner).normalized;
		Vector3 _videoOutputForward = Vector3.Cross((augmentaVideoOutput.topRightCorner - augmentaVideoOutput.topLeftCorner).normalized, _videoOutputUp).normalized;

		transform.rotation = Quaternion.LookRotation(_videoOutputForward, _videoOutputUp);
	}
}
