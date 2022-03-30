using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquirrelTarget : MonoBehaviour
{
	public Augmenta.AugmentaObject augmentaObject {
		get { if (!_augmentaObject)
				GetAugmentaObject();
			return _augmentaObject;
		}
	}

	private Augmenta.AugmentaObject _augmentaObject;

	private void OnEnable() {

		SquirrelsManager.instance.targets.Add(this);
	}

	private void OnDisable() {

		SquirrelsManager.instance.targets.Remove(this);
	}

	private void GetAugmentaObject() {

		_augmentaObject = transform.parent.GetComponentInChildren<Augmenta.AugmentaObject>();
	}
}
