using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Arex;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using InputObservable;

namespace ARClock
{
    public class ARClock : MonoBehaviour
    {
        [Inject] IARPlaneRaycastManager planeRaycastManager = default;
        [Inject] IARPlaneManager planeManager = default;
        [Inject] IARCamera arcamera = default;

        [SerializeField] GameObject clockPrefab = default;

        IARPlane wall = null;
        bool placed = false;

        void Start()
        {
            this.planeManager.EnableSearchPlanes = true;
            this.DefaultInputContext().GetObservable(0).OnBegin.Subscribe(e => { 
                if(placed)
                    return;
                if (planeRaycastManager.Raycast(e.position, out Arex.RaycastHit hit))
                {
                    foreach (var plane in planeManager.ActivePlanes())
                    {
                        plane.visible = false;
                    }
                    Instantiate(clockPrefab, hit.pose.position,
                        Quaternion.LookRotation(arcamera.transform.position - hit.pose.position));
                    // hit.pose.rotation);
                    placed = true;
                }
            }).AddTo(this);
        }
    }
}
