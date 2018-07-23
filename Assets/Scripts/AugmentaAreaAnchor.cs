using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaAreaAnchor : MonoBehaviour {

    [Header("Augmenta area visualizer (gizmo only)")]
    public float Width;
    public float Height;
    public float PixelMeterCoeff;
    public bool DrawGizmos;

	// Update is called once per frame
	void Update () {
        AugmentaArea.Instance.gameObject.transform.position = transform.position;
        AugmentaArea.Instance.gameObject.transform.rotation = transform.rotation;
    }

    private void OnDrawGizmos()
    {
        if (!DrawGizmos) return;

        Gizmos.color = Color.blue;
        //Draw area 
        var ratio = (Width / Height);
        var scale = new Vector3(ratio * PixelMeterCoeff, 1 * PixelMeterCoeff, 0.1f);

        DrawGizmoCube(transform.position, transform.rotation, scale);
    }

    public void DrawGizmoCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;
    }
}
