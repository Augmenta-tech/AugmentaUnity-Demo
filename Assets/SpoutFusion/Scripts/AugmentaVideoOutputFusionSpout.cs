using Augmenta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaVideoOutputFusionSpout : MonoBehaviour
{
	[Header("Augmenta")]
	public AugmentaVideoOutput augmentaVideoOutput;

	[Header("Spout")]
	public Klak.Spout.SpoutReceiver spoutReceiver;
	public Renderer spoutRenderer;

	public bool showFusionSpout = false;
	public bool autoFindFusionSpout = true;
	public string fusionSpoutName = "Augmenta Fusion - Scene";

	private RenderTexture _spoutTexture;
	private GameObject _spoutObject;

	private bool _initialized = false;

	private void OnEnable() {

		InitializeSpout();
	}

	private void Update() {

		if (showFusionSpout && !_spoutObject.activeSelf) {
			_spoutObject.SetActive(true);
		} else if (!showFusionSpout && _spoutObject.activeSelf) {
			_spoutObject.SetActive(false);
		}


		if (showFusionSpout) {
			if (!_initialized || augmentaVideoOutput.videoOutputSizeInPixels.x != _spoutTexture.width || augmentaVideoOutput.videoOutputSizeInPixels.y != _spoutTexture.height)
				InitializeSpout();

			UpdateSpoutObject();
		}
	}

	private void OnDisable() {

		DisableSpout();
	}

	void InitializeSpout() {

		//Get spout object
		_spoutObject = spoutRenderer.gameObject;

		//Set spout source name
		if (autoFindFusionSpout) {
			foreach (var source in Klak.Spout.SpoutManager.GetSourceNames()) {
				if (source.Contains("Augmenta Fusion")) {
					spoutReceiver.sourceName = source;
					break;
				}
			}
		} else {
			spoutReceiver.sourceName = fusionSpoutName;
		}

		//If video output has 0 size, abort
		if (augmentaVideoOutput.videoOutputSizeInPixels.x == 0 || augmentaVideoOutput.videoOutputSizeInPixels.y == 0)
			return;

		//Create spout texture
		_spoutTexture = new RenderTexture(augmentaVideoOutput.videoOutputSizeInPixels.x, augmentaVideoOutput.videoOutputSizeInPixels.y, 0, RenderTextureFormat.ARGB32);
		//Assign texture to spout receiver
		spoutReceiver.targetTexture = _spoutTexture;
		//Assign texture to spout display
		spoutRenderer.sharedMaterial.SetTexture("_MainTex", _spoutTexture);

		_initialized = true;
	}

	void DisableSpout() {

		if (_spoutTexture)
			_spoutTexture.Release();
	}

	void UpdateSpoutObject() {

		//Place spout display
		transform.position = augmentaVideoOutput.botLeftCorner + 0.5f * (augmentaVideoOutput.topLeftCorner - augmentaVideoOutput.botLeftCorner) + 0.5f * (augmentaVideoOutput.topRightCorner - augmentaVideoOutput.topLeftCorner);
		transform.localScale = new Vector3(Vector3.Distance(augmentaVideoOutput.botRightCorner, augmentaVideoOutput.botLeftCorner), Vector3.Distance(augmentaVideoOutput.topLeftCorner, augmentaVideoOutput.botLeftCorner), 1);

		Vector3 _videoOutputUp = (augmentaVideoOutput.topLeftCorner - augmentaVideoOutput.botLeftCorner).normalized;
		Vector3 _videoOutputForward = Vector3.Cross((augmentaVideoOutput.topRightCorner - augmentaVideoOutput.topLeftCorner).normalized, _videoOutputUp).normalized;

		transform.rotation = Quaternion.LookRotation(_videoOutputForward, _videoOutputUp);
	}
}
