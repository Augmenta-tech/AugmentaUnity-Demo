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
    private float _previousMessageTime = 0;
    private List<float> _mpsList = new List<float>();

	private void OnEnable() {

        augmentaManager.sceneUpdated += OnAugmentaSceneMessageReceived;
	}

    private void OnDisable() {

        augmentaManager.sceneUpdated -= OnAugmentaSceneMessageReceived;
    }

	void OnAugmentaSceneMessageReceived() {

        //_messageCount++;

        augmentaMPS = 1.0f / (Time.time - _previousMessageTime);

        _previousMessageTime = Time.time;

        text.text = "Messages per seconds = " + augmentaMPS.ToString("F1");
    }
}
