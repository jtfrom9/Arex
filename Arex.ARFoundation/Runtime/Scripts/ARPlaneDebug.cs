using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
using Arex;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(ARPlane))]
    [RequireComponent(typeof(ArexARFoundationPlane))]
    public class ARPlaneDebug : MonoBehaviour
    {
        public TrackingState trackingState;
        public string trackableId;
        public string subsumedBy;
        public Vector3 center;


        public float area;
        public Vector2[] boundary;

        ARPlane nativePlane;
        IARPlane plane;

        void Awake()
        {
            nativePlane = GetComponent<ARPlane>();
            plane = GetComponent<ArexARFoundationPlane>() as IARPlane;

            Observable.FromEvent<ARPlaneBoundaryChangedEventArgs>(h => nativePlane.boundaryChanged += h, h => nativePlane.boundaryChanged -= h)
                .Subscribe(e => {
                    area = plane.GetArea();
                    boundary = nativePlane.boundary.ToArray();
                }).AddTo(this);

            Assert.IsNotNull(nativePlane);
            Assert.IsNotNull(plane);
        }

        void Update()
        {
            trackingState = nativePlane.trackingState;
            trackableId = nativePlane.trackableId.ToString();
            subsumedBy = (nativePlane.subsumedBy != null) ? nativePlane.subsumedBy.trackableId.ToString() : "";
            center = nativePlane.center;
            gameObject.name = plane.ToShortStrig();
        }
    }
}
