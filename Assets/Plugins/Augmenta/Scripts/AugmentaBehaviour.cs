using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaBehaviour : MonoBehaviour {

    public int pid;
    public float AbstractValue;

    [Header("Appeareance animation")]
    public float AppearAnimDuration;
    public float AppearStartValue;
    public float AppearEndValue;
    public AnimationCurve AppearAnimCurve;

    [Header("Alive animation")]
    public float AliveAnimDuration;
    public float AliveStartValue;
    public float AliveEndValue;
    public AnimationCurve AliveAnimCurve;
    public bool LoopAliveAnimation;

    [Header("Disappeareance animation")]
    public float DisappearAnimDuration;
    public float DisappearStartValue;
    public float DisappearEndValue;
    public bool StartWithActualValue;
    public AnimationCurve DisappearAnimCurve;

    public delegate void DisappearAnimationCompleted(int pid);
    public event DisappearAnimationCompleted disappearAnimationCompleted;

    public virtual IEnumerator ValueAnimation(float startValue, float endValue, float duration, AnimationCurve animCurve = null, System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while(currentTime < duration)
        {
            if (animCurve != null)
                AbstractValue = Mathf.Lerp(startValue, endValue, animCurve.Evaluate(currentTime / duration));
            else
                AbstractValue = Mathf.Lerp(startValue, endValue, currentTime / duration);

            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        AbstractValue = endValue;

        if (callBack != null)
            callBack();

        yield return null;
    }

    public virtual void AliveCallBack()
    {
        if(LoopAliveAnimation)
            StartCoroutine(ValueAnimation(AliveStartValue, AliveEndValue, AliveAnimDuration, AliveAnimCurve, AliveCallBack));
    }

    public virtual void AppearCallBack()
    {
        StartCoroutine(ValueAnimation(AbstractValue, AliveEndValue, AliveAnimDuration, AliveAnimCurve, AliveCallBack));
    }

    public virtual void DisappearCallBack()
    {
        if (disappearAnimationCompleted != null)
            disappearAnimationCompleted(pid);
    }

    public void Disappear()
    {
        if(StartWithActualValue)
            StartCoroutine(ValueAnimation(AbstractValue, DisappearEndValue, DisappearAnimDuration, DisappearAnimCurve, DisappearCallBack));
        else
            StartCoroutine(ValueAnimation(DisappearStartValue, DisappearEndValue, DisappearAnimDuration, DisappearAnimCurve, DisappearCallBack));
    }

    public void Appear()
    {
        StartCoroutine(ValueAnimation(AppearStartValue, AppearEndValue, AppearAnimDuration, AppearAnimCurve, AppearCallBack));
    }
}
