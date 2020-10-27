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
        public Transform cameraTransform;
        public Button button;
        public DebugPanel debugPanel;

        PlaneScanner planeScanner;
        float maxSquare = 0;

        async UniTask scanPlane(CancellationToken token)
        {
            var ret = await planeScanner.Scan(
                numPlanes: 30,
                timeout: 10, token: token, waitFirstPlane: true);
            if ((ret.result == PlaneScanResult.Found || ret.result == PlaneScanResult.Timeout) && ret.planesFound > 0)
            {
                var orderedPlanes = planeScanner.planeManager.ActivePlanes().OrderByDescending(p => p.size.x * p.size.y);
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
                        // plane.SetFlag(ARPlaneDebugFlag.OutlineOnly);
                        plane.visible = false;
                    }
                }
                debugPanel.PrintLog($"<color=red>max lowest is #{maxPlane.id}</color>");
            }
            else
            {
                debugPanel.PrintLog($"{ret.result.ToString()} ({ret.message})");
            }
        }

        CancellationToken watchCameraMove(CompositeDisposable disposable)
        {
            var currentPosition = cameraTransform.position;
            var cts = new CancellationTokenSource();
            this.UpdateAsObservable().Subscribe(_ =>
            {
                var distance = (currentPosition - cameraTransform.position).magnitude;
                if (distance > 3.0f)
                {
                    debugPanel.PrintLog("Moved too far. cancel plane scan");
                    cts.Cancel();
                    cts.Dispose();
                    cts = null;
                }
                else if (distance > 1.2f)
                {
                    debugPanel.PrintLog($"Stay and scan your surroundings. moved {distance} meters");
                }
            }).AddTo(disposable);
            return cts.Token;
        }

        void Start()
        {
            planeScanner = GetComponent<PlaneScanner>();
            button.OnClickAsObservable().Subscribe(async _ => {
                debugPanel.PrintLog($"StartScan");

                button.interactable = false;
                var disposable = new CompositeDisposable();
                await scanPlane(watchCameraMove(disposable));
                disposable.Dispose();
                button.interactable = true;

                debugPanel.PrintLog($"Scan Ended");
            }).AddTo(this);
        }
    }
}
