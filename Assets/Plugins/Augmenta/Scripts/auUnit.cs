using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class auUnit : MonoBehaviour {

	/*
	 * The auUnit can be used as a reference to make visuals' scale (and other parameters) relative to real world scale.
	 * The auUnit must be setup when calibrating the software for videoprojection.
	 * The white plane representing it in debug mode represent 1 meter in real world. 
	 */

	/// Real-people relative unit
	public static float unit = 0.3f;
	/// Value used to compare to current auUnit
	private static float previousAuUnit = 0;

	/// Delegate called when auUnit changed.
	public delegate void OnAuUnitChanged ();
	/// Delegate called when auUnit changed.
	public static OnAuUnitChanged onAuUnitChanged;

	/// GameObject representing auUnit scale
	public GameObject unitPlane;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (previousAuUnit != unit) {
			if (onAuUnitChanged != null) {
				onAuUnitChanged ();
			}
			previousAuUnit = unit;
		}

		// Adjust auUnit scale
		if (unitPlane.transform.localScale.x != unit
			|| unitPlane.transform.localScale.z != unit) {
			unitPlane.transform.localScale = new Vector3 (unit, 1, unit);
		}
	}
}
