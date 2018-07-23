using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedTextureControllable : Controllable
{
    [OSCProperty] public bool SpoutOutput;

    public SharedTexture MySharedTexture;
 
    // Use this for initialization
    public override void Awake()
    {

        if (MySharedTexture == null)
            MySharedTexture = FindObjectOfType<SharedTexture>();

        if (MySharedTexture == null)
        {
            Debug.LogWarning("Can't find SharedTexture script to control !");
            return;
        }

        TargetScript = MySharedTexture;
        usePresets = true;
        base.Awake();
    }

    public override void OnScriptValueChanged(string name)
    {
        base.OnScriptValueChanged(name);
        SpoutOutput = MySharedTexture.enabled;
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);
        MySharedTexture.enabled = SpoutOutput;
    }
}

