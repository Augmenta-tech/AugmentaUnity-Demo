using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAugmentaBehaviour : AugmentaPersonBehaviour {

    public float scale;

    [Header("Appeareance animation")]
    public float AppearAnimDuration;
    public AnimationCurve AppearAnimCurve;

    [Header("Alive animation")]
    public float AliveAnimDuration;
    public AnimationCurve AliveAnimCurve;

    [Header("Disappeareance animation")]
    public float DisappearAnimDuration;
    public bool StartWithActualValue;
    public AnimationCurve DisappearAnimCurve;

    private void Start()
    {
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        transform.localScale = Vector3.one * scale;
    }

    protected override IEnumerator AppearAnimation(System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while (currentTime < AppearAnimDuration)
        {
            currentTime += Time.deltaTime;

            if (AppearAnimCurve != null)
                scale = AppearAnimCurve.Evaluate(Mathf.Clamp01(currentTime / AppearAnimDuration));
            else
                scale = Mathf.Clamp01(currentTime / AppearAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }

    protected override IEnumerator AliveAnimation(System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while (currentTime < AliveAnimDuration)
        {
            currentTime += Time.deltaTime;

            if (AliveAnimCurve != null)
                scale = AliveAnimCurve.Evaluate(Mathf.Clamp01(currentTime / AliveAnimDuration));
            else
                scale = Mathf.Clamp01(currentTime / AliveAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }

    protected override IEnumerator DisappearAnimation(System.Action callBack = null)
    {
        if (StartWithActualValue)
        {
            DisappearAnimCurve.MoveKey(0, new Keyframe(0.0f, scale));
        }

        var currentTime = 0.0f;
        while (currentTime < DisappearAnimDuration)
        {
            currentTime += Time.deltaTime;

            if (DisappearAnimCurve != null)
                scale = DisappearAnimCurve.Evaluate(Mathf.Clamp01(currentTime / DisappearAnimDuration));
            else
                scale = Mathf.Clamp01(currentTime / DisappearAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }
}
