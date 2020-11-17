using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Arex.ARFoundation
{
    public class ArexARFoundationARCamera : MonoBehaviour, IARCamera
    {
        ARSessionOrigin origin;

        void Awake()
        {
            origin = GetComponent<ARSessionOrigin>();
        }

        Transform IARCamera.transform
        {
            get => origin.camera.transform;
        }
    }
}