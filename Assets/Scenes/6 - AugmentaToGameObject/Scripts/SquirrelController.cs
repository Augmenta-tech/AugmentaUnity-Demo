using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquirrelController : MonoBehaviour
{
    [Header("Squirrels Parameters")]
    public float speed = 2.0f;
    public float minPointDistance = 1.0f;
    public float detectionDistance = 5.0f;

    [Header("Gizmos")]
    public bool showGizmos = false;

    private Animator _animator;

    private SquirrelTarget[] _targets;
    private SquirrelTarget _target;
    private Vector3 _targetDirection;

    private float _closestTargetDistance = 100000.0f;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        UpdateAnimation(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSquirrelBehaviour();
    }

    void UpdateAnimation(bool moving) {

        _animator.SetBool("Moving", moving);
    }

    void UpdateSquirrelBehaviour() {

        //Find all targets
        _targets = FindObjectsOfType<SquirrelTarget>();

        if(_targets.Length == 0) {
            //If no target found, go to idle animation
            _target = null;
            UpdateAnimation(false);

		} else {
            //Get closest target
            _closestTargetDistance = 100000.0f;
            foreach(SquirrelTarget target in _targets) {
                float _distance = Vector3.Distance(transform.position, target.transform.position);
                if (_distance < _closestTargetDistance) {
                    _target = target;
                    _closestTargetDistance = _distance;
				}
			}

            _targetDirection = (_target.transform.position - transform.position).normalized;

            //Check closest target distance
            if (_closestTargetDistance <= minPointDistance || _closestTargetDistance > detectionDistance) {
                //If close enough stop moving
                UpdateAnimation(false);

            } else {
                //Else move to closest target
                transform.position = transform.position + (_targetDirection * speed * Time.deltaTime);
                UpdateAnimation(true);
            }
            //Look at closest target
            transform.rotation = Quaternion.LookRotation(_targetDirection, Vector3.up);
        }
    }

	private void OnDrawGizmos() {

        if (!showGizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _targetDirection);
	}
}
