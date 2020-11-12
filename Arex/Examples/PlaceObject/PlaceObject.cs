using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;
using InputObservable;
using Zenject;

namespace Arex.Examples
{
    public class PlaceObject : MonoBehaviour
    {
        [SerializeField] GameObject prefab = default;

        [Inject] IARPlaneManager planeManager = default;
        [Inject] IARPlaneRaycastManager raycastManager = default;

        void Start()
        {
            this.planeManager.EnableSearchPlanes = true;

            var ictx = this.DefaultInputContext();
            var io = ictx.GetObservable(0);
            io.OnBegin.Subscribe(e => {
                Debug.Log($"touch: {e}");
                RaycastHit hit;
                if(raycastManager.Raycast(e.position, out hit)) {
                    // Instantiate(prefab, hit.pose.position, hit.pose.rotation);
                    Instantiate(prefab, hit.plane.GetAnchor(hit.pose));
                }
            }).AddTo(this);
        }
    }
}