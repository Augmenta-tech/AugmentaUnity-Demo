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

	private void GetAugmentaObject() {

		_augmentaObject = transform.parent.GetComponentInChildren<Augmenta.AugmentaObject>();
	}
}
