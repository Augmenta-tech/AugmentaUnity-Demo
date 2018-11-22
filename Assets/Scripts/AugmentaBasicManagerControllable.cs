using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaBasicManagerControllable : Controllable {

    [OSCProperty]
    public float PointTimeOut;

    [OSCProperty]
    [Range(1, 20)]
    public float PositionFollowTightness;

    [OSCProperty]
    [Range(1, 20)]
    public int VelocityAverageValueCount;
}
