using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBehaviour : AugmentaBasicPersonBehaviour
{

	private Vector3 startingScale;

	private void Awake() {
		//Save the starting scale value
		startingScale = transform.localScale;

		//Set the scale to 0 so the cube is invisible before starting its appearing animation
		transform.localScale = Vector3.zero;
	}

	void Update()
    {
		//Link the animated value from the AugmentaBasicPersonBehaviour to the transform scale.
		transform.localScale = startingScale * animatedValue;
    }
}
