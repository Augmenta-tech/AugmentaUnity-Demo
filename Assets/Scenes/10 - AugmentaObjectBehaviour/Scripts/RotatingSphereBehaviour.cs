using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Augmenta;

public class RotatingSphereBehaviour : MonoBehaviour, IAugmentaObjectBehaviour
{
    public float rotationSpeed = 90;
    public float transitionDuration = 2;
    public AnimationCurve transitionCurve;

    private Vector3 _defaultScale;

    private bool _isSpawning = false;
    private Coroutine _spawnCoroutine;

	#region MonoBehaviour

	void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

	#endregion

	#region AugmentaObjectBehaviour

    public void Spawn() {

        _spawnCoroutine = StartCoroutine(SpawnCoroutine());
	}

    public void Destroy() {

        if (_isSpawning)
            StopCoroutine(_spawnCoroutine);

        StartCoroutine(DestroyCoroutine());
	}

	#endregion

	#region Transitions Coroutines

	IEnumerator SpawnCoroutine() {

        _isSpawning = true;

        //Save current scale
        _defaultScale = transform.localScale;

        //Start at scale zero
        transform.localScale = Vector3.zero;

        float timer = 0;

        //Scale up to default scale
        while(timer < transitionDuration) {
            timer += Time.deltaTime;
            timer = Mathf.Min(timer, transitionDuration);
            transform.localScale = _defaultScale * transitionCurve.Evaluate(timer / transitionDuration);
            yield return new WaitForEndOfFrame();
		}

        _isSpawning = false;
	}

    IEnumerator DestroyCoroutine() {

        //Save current scale
        _defaultScale = transform.localScale;

        float timer = 0;

        //Scale down to zero
        while (timer < transitionDuration) {
            timer += Time.deltaTime;
            timer = Mathf.Min(timer, transitionDuration);
            transform.localScale = Vector3.Lerp(_defaultScale, Vector3.zero, transitionCurve.Evaluate(timer / transitionDuration));
            yield return new WaitForEndOfFrame();
        }

        //Destroy game object
        Destroy(gameObject);
    }

	#endregion
}
