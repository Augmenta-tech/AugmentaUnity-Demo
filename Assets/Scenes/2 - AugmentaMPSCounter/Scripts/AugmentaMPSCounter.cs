using Augmenta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AugmentaMPSCounter : MonoBehaviour
{
    public AugmentaManager augmentaManager;

    public Text text;

    public float calculationWindow = 1.0f;

    public float augmentaMPS;

    private int _messageCount = 0;
    private float _timer = 0;

	private void OnEnable() {

        augmentaManager.sceneUpdated += OnAugmentaSceneMessageReceived;

        _timer = 0;
	}

	private void Update() {

        _timer += Time.deltaTime;

        if(_timer >= calculationWindow) {
            augmentaMPS = _messageCount / _timer;
            text.text = "Messages per seconds = " + augmentaMPS.ToString("F1");
            _messageCount = 0;
            _timer = 0;
        }
	}

	private void OnDisable() {

        augmentaManager.sceneUpdated -= OnAugmentaSceneMessageReceived;
    }

	void OnAugmentaSceneMessageReceived() {

        _messageCount++;
    }
}
