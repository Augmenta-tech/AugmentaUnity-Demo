using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Augmenta;

public class AugmentaBasicManager : MonoBehaviour {

    public GameObject PrefabToInstantiate;

    public Dictionary<int, GameObject> InstantiatedObjects;

    public bool UsePositionTweening;
    public float PositionFollowTightness;
    public int VelocityAverageValueCount;

    public virtual void Update()
    {
        if (!UsePositionTweening)
            return;

        foreach (var element in InstantiatedObjects)
        {
            if (!AugmentaArea.AugmentaPoints.ContainsKey(element.Key)) continue;

            element.Value.transform.position = Vector3.Lerp(element.Value.transform.position, AugmentaArea.AugmentaPoints[element.Key].Position, Time.deltaTime * PositionFollowTightness);
        }
    }
	// Use this for initialization
	public virtual void OnEnable () {
        InstantiatedObjects = new Dictionary<int, GameObject>();

        AugmentaArea.personEntered += PersonEntered;
        AugmentaArea.personUpdated += PersonUpdated;
        AugmentaArea.personLeaving += PersonLeft;
        AugmentaArea.sceneUpdated += SceneUpdated;
    }

    // Use this for initialization
    public virtual void OnDisable()
    {
        foreach (var element in InstantiatedObjects.Values)
            Destroy(element);

        InstantiatedObjects.Clear();

        AugmentaArea.personEntered -= PersonEntered;
        AugmentaArea.personUpdated -= PersonUpdated;
        AugmentaArea.personLeaving -= PersonLeft;
        AugmentaArea.sceneUpdated -= SceneUpdated;
    }

    public virtual void SceneUpdated(AugmentaScene s)
    { }

    public virtual void PersonEntered(AugmentaPerson p)
    {
        if(!InstantiatedObjects.ContainsKey(p.pid))
        {
            var newObject = Instantiate(PrefabToInstantiate, p.Position, Quaternion.identity, this.transform);
            InstantiatedObjects.Add(p.pid, newObject);

            var augBehaviour = newObject.GetComponent<AugmentaBehaviour>();
            if (augBehaviour != null)
            {
                augBehaviour.pid = p.pid;
                augBehaviour.disappearAnimationCompleted += HandleDisappearedObject;
                augBehaviour.Appear();
            }
        }
    }

    public virtual void PersonUpdated(AugmentaPerson p)
    {
        if (InstantiatedObjects.ContainsKey(p.pid))
        {
            p.VelocitySmooth = VelocityAverageValueCount;

            if (!UsePositionTweening)
                InstantiatedObjects[p.pid].transform.position = p.Position;
        }
        else
        {
            PersonEntered(p);
        }
    }

    public virtual void PersonLeft(AugmentaPerson p)
    {
        if (InstantiatedObjects.ContainsKey(p.pid))
        {
            var augBehaviour = InstantiatedObjects[p.pid].GetComponent<AugmentaBehaviour>();
            if (augBehaviour != null)
                augBehaviour.Disappear();
            else
                HandleDisappearedObject(p.pid);
        }
    }

    public virtual void HandleDisappearedObject(int pid)
    {
        if (!InstantiatedObjects.ContainsKey(pid)) //To investigate, shouldn't happen
            return;

        var objectToDestroy = InstantiatedObjects[pid];
        Destroy(objectToDestroy);
        InstantiatedObjects.Remove(pid);
    }
}
