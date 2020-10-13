using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Arex;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(ARPlane))]
    [RequireComponent(typeof(ARFoundationPlane))]
    public class ARPlaneDebug : MonoBehaviour
    {
        public TrackingState trackingState;
        public string trackableId;
        public string subsumedBy;

        ARPlane nativePlane;
        IPlane plane;

        void Awake()
        {
            nativePlane = GetComponent<ARPlane>();
            plane = GetComponent<ARFoundationPlane>() as IPlane;

            Assert.IsNotNull(nativePlane);
            Assert.IsNotNull(plane);
        }

        void Update()
        {
            trackingState = nativePlane.trackingState;
            trackableId = nativePlane.trackableId.ToString();
            subsumedBy = (nativePlane.subsumedBy != null) ? nativePlane.subsumedBy.trackableId.ToString() : "";

            gameObject.name = plane.ToShortStrig();
        }
    }
}
