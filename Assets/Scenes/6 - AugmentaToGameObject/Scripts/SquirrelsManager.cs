using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Augmenta;

public class SquirrelsManager : MonoBehaviour
{
    public static SquirrelsManager instance;

    [HideInInspector] public List<SquirrelTarget> targets = new List<SquirrelTarget>();

	private void Awake() {
		instance = this;
	}
}
