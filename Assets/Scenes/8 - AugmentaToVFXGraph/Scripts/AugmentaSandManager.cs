using Augmenta;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AugmentaSandManager : MonoBehaviour
{
    public AugmentaManager augmentaManager;
    public VisualEffect vfx;

	private Vector3 _defaultPosition = 1000000.0f * Vector3.one;

	// Update is called once per frame
	private void OnEnable() {

		InitializePositions();

		augmentaManager.augmentaObjectUpdate += OnAugmentaObjectUpdate;
		augmentaManager.augmentaObjectLeave += OnAugmentaObjectLeave;
	}

	void OnDisable() {

		augmentaManager.augmentaObjectUpdate -= OnAugmentaObjectUpdate;
		augmentaManager.augmentaObjectLeave -= OnAugmentaObjectLeave;
	}

	void InitializePositions() {

		vfx.SetVector3("_AugmentaObjectPosition0", _defaultPosition);
		vfx.SetVector3("_AugmentaObjectPosition1", _defaultPosition);
		vfx.SetVector3("_AugmentaObjectPosition2", _defaultPosition);
	}

	void OnAugmentaObjectUpdate(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType) {

		if (augmentaDataType != AugmentaDataType.Main)
			return;

		switch (augmentaObject.oid) {
			case 0:
				vfx.SetVector3("_AugmentaObjectPosition0", augmentaObject.worldPosition2D);
				break;

			case 1:
				vfx.SetVector3("_AugmentaObjectPosition1", augmentaObject.worldPosition2D);
				break;

			case 2:
				vfx.SetVector3("_AugmentaObjectPosition2", augmentaObject.worldPosition2D);
				break;
		}
	}

	void OnAugmentaObjectLeave(AugmentaObject augmentaObject, AugmentaDataType augmentaDataType) {

		if (augmentaDataType != AugmentaDataType.Main)
			return;

		switch (augmentaObject.oid) {
			case 0:
				vfx.SetVector3("_AugmentaObjectPosition0", _defaultPosition);
				break;

			case 1:
				vfx.SetVector3("_AugmentaObjectPosition1", _defaultPosition);
				break;

			case 2:
				vfx.SetVector3("_AugmentaObjectPosition2", _defaultPosition);
				break;
		}
	}
}
