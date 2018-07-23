using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAugmentaBehaviour : AugmentaBehaviour {

    void Start()
    {
        transform.localScale = Vector3.zero;
    }

    void Update () {
        transform.localScale = new Vector3(AbstractValue, AbstractValue, AbstractValue);
	}
}
