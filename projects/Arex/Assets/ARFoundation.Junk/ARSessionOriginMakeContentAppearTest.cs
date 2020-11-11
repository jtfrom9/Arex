using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSessionOriginMakeContentAppearTest : MonoBehaviour
{
    public ARSessionOrigin origin;
    public Transform arcamera;
    public Transform target;

    Vector3 lastPosition = Vector3.zero;

    void Start()
    {
        origin.MakeContentAppearAt(content: origin.transform,
            position: new Vector3 { x = 0, y = -0.1f, z = 0 },
            rotation: Quaternion.Euler(0, -90, 0));

        // origin.MakeContentAppearAt(content: target,
        //     position: new Vector3 { x = 0, y = 0, z = 10 },
        //     rotation: Quaternion.Euler(0, -90, 0));

    }

    void Update()
    {
        // Debug.Log($"cam: {arcamera.rotation.eulerAngles}, {arcamera.localRotation.eulerAngles}");

        // var diff = arcamera.localPosition - lastPosition;
        var diff = arcamera.position - lastPosition;
        var dx = Mathf.Abs(diff.x);
        var dz = Mathf.Abs(diff.z);
        float e = 0.001f;
        if (dx > e && dz > e)
        {
            if (dx > dz)
            {
                Debug.Log($"<color=red>X={diff.x}</color>");
            }
            else
            {
                Debug.Log($"<color=blue>Z={diff.z}</color>");
            }
        }
        // lastPosition = arcamera.localPosition;
        lastPosition = arcamera.position;
    }
}
