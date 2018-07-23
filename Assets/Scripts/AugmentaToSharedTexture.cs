using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaToSharedTexture : MonoBehaviour {

    public SharedTexture MySharedTexture;

	// Use this for initialization
	void Start () {
        if(MySharedTexture == null)
            MySharedTexture = FindObjectOfType<SharedTexture>();

        AugmentaArea.sceneUpdated += SceneUpdated;
    }
    
    public void SceneUpdated(AugmentaScene s)
    {
        if (MySharedTexture != null)
            MySharedTexture.NewTextureSize((int)s.Width, (int)s.Height);
    }

}
