using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

namespace Arex.Examples
{
    [RequireComponent(typeof(PlaneScanner))]
    public class GroundFinder : MonoBehaviour
    {
        public Button button;

        PlaneScanner planeScanner;
        float maxSquare = 0;

        async UniTask scanPlane(CancellationToken token)
        {
            var condition = PlaneConditionMatcher.IsValidPlane;
            var ret = await planeScanner.StartScan(newPlanes: 30, timeout: 10, token: token,
                condition: condition,
                waitFirstPlane: true);
            if ((ret.result == PlaneScanResult.Found || ret.result == PlaneScanResult.Timeout) && ret.planesFound > 0)
            {
                var planes = planeScanner.planeManager.planes.Where(p => condition(p));
                var orderedPlanes = planes.OrderByDescending(p => p.size.x * p.size.y);
                var maxPlane = orderedPlanes.FirstOrDefault();
                var maxSquare = maxPlane.size.x * maxPlane.size.y;
                var lowestPlane = maxPlane;

                foreach (var plane in orderedPlanes)
                {
                    var square = plane.size.x * plane.size.y;
                    if (square < maxSquare * 0.75f)
                    {
                        break;
                    }
                    if (plane.center.y < lowestPlane.center.y)
                    {
                        lowestPlane = plane;
                    }
                }
                foreach (var plane in planeScanner.planeManager.planes)
                {
                    if (plane != lowestPlane)
                    {
                        plane.SetDebug(ARPlaneDebugFlag.OutlineOnly);
                    }
                }
                Debug.Log($"<color=red>max lowest is #{maxPlane.id}</color>");
            }
            else
            {
                Debug.LogWarning($"{ret.result.ToString()} ({ret.message})");
            }
        }

        CancellationToken watchCameraMove(CompositeDisposable disposable)
        {
            var currentPosition = Camera.main.transform.position;
            var cts = new CancellationTokenSource();
            this.UpdateAsObservable().Subscribe(_ => {
                var distance = (currentPosition - Camera.main.transform.position).magnitude;
                if (distance > 3.0f)
                {
                    Debug.LogError("Moved too far. cancel plane scan");
                    cts.Cancel();
                    cts.Dispose();
                } else if (distance > 1.0f)
                {
                    Debug.Log($"Please stay in the same place and scan your surroundings. you already moved {distance} meters");
                }
            }).AddTo(disposable);
            return cts.Token;
        }

        void Start()
        {
            planeScanner = GetComponent<PlaneScanner>();
            // token = this.GetCancellationTokenOnDestroy();

            button.OnClickAsObservable().Subscribe(async _ => {
                button.interactable = false;
                var disposable = new CompositeDisposable();
                await scanPlane(watchCameraMove(disposable));
                disposable.Dispose();
                button.interactable = true;
            }).AddTo(this);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
