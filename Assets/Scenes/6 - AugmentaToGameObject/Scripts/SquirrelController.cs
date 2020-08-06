using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class SquirrelController : MonoBehaviour
{
    [Header("Squirrels Parameters")]
    public float speed = 2.0f;
    public float detectionDistance = 5.0f;
    public float minPointDistanceHighHeight = 3.0f;
    public float minPointDistanceLowHeight = 0.9f;
    public float heightThreshold = 1.0f;
    public float minTimeBetweenTargetChange = 1.0f;

    [Header("Gizmos")]
    public bool showGizmos = false;

    private Animator _animator;

    private SquirrelTarget[] _targets;
    private SquirrelTarget _target;
    private Vector3 _targetDirection;

    private float _closestTargetDistance = 100000.0f;
    private float _minPointDistance;

    private enum State { Idle, MovingCloser, MovingAway }
    private State _currentState = State.Idle;

    private float _timeSinceLastTargetChange = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        GoToIdle();
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

        //Increase target timer
        _timeSinceLastTargetChange += Time.deltaTime;

        //Find all targets
        _targets = FindObjectsOfType<SquirrelTarget>();

        if(_targets.Length == 0) {
            //If no target found, go to idle animation
            _target = null;
            GoToIdle();

        } else {
            //If no target or have been following target long enough (to avoid target flickering)
            if (_target == null || _timeSinceLastTargetChange > minTimeBetweenTargetChange) {
                //Get closest target
                _closestTargetDistance = 100000.0f;
                foreach (SquirrelTarget target in _targets) {
                    float _distance = Vector3.Distance(transform.position, target.transform.position);
                    if (_distance < _closestTargetDistance) {
                        _target = target;
                        _closestTargetDistance = _distance;
                    }
                }

                _timeSinceLastTargetChange = 0.0f;
            }

            _targetDirection = (_target.transform.position - transform.position).normalized;
            _minPointDistance = _target.augmentaObject.highest.z > heightThreshold ? minPointDistanceHighHeight : minPointDistanceLowHeight;

			switch (_currentState) {
                case State.Idle:

                    if(_closestTargetDistance < detectionDistance) {
                        //If in detection distance
                        if (_closestTargetDistance > _minPointDistance * 1.3f) {
                            //Move closer if too far
                            GoToMovingCloser();
                        } else if (_closestTargetDistance < _minPointDistance * 0.7f) {
                            //Move away if too close
                            GoToMovingAway();
						}
                    }
                    
                    break;

                case State.MovingAway:

                    if(_closestTargetDistance > _minPointDistance) {
                        //Far enough, go to idle
                        GoToIdle();
					}

                    break;

                case State.MovingCloser:

                    if (_closestTargetDistance < _minPointDistance) {
                        //Close enough, go to idle
                        GoToIdle();
                    }

                    break;
			}
        }

        UpdatePosition();
        UpdateRotation();
    }

    void UpdatePosition() {

        if(_currentState == State.MovingCloser)
            transform.position += _targetDirection * speed * Time.deltaTime;
        else if(_currentState == State.MovingAway)
            transform.position -= _targetDirection * speed * Time.deltaTime;
    }

    void UpdateRotation() {

            transform.rotation = Quaternion.LookRotation(_currentState == State.MovingAway ? -_targetDirection : _targetDirection, Vector3.up);
	}

    void GoToIdle() {

        _currentState = State.Idle;

        UpdateAnimation(false);
    }

    void GoToMovingCloser() {

        _currentState = State.MovingCloser;

        UpdateAnimation(true);
    }

    void GoToMovingAway() {

        _currentState = State.MovingAway;

        UpdateAnimation(true);
    }

	private void OnDrawGizmos() {

        if (!showGizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _targetDirection);
	}
}
