using Klak.Spout;
using Augmenta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaVideoOutputSpoutBinder : MonoBehaviour
{
    public SpoutSender spoutSender;
    public AugmentaVideoOutput augmentaVideoOutput;

    // Start is called before the first frame update
    void OnEnable()
    {
        augmentaVideoOutput.videoOutputTextureUpdated += OnVideoOutputTextureUpdated;
        OnVideoOutputTextureUpdated();
    }

	void OnDisable() {

        augmentaVideoOutput.videoOutputTextureUpdated -= OnVideoOutputTextureUpdated;
    }

	void OnVideoOutputTextureUpdated() {

        spoutSender.sourceTexture = augmentaVideoOutput.videoOutputTexture;
	}
}
