using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Arex.Examples
{
    public class PlaneScannerMain : MonoBehaviour
    {
        public PlaneScanner planeScanner;

        public Button button;
        public DebugPanel debugPanel;

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        void printLog(string msg)
        {
            debugPanel.PrintLog(msg);
        }

        async UniTask scan()
        {
            int planes = 3;
            int timeout = 10;
            printLog($"Start Scan Plane (planes: {planes}, timeout: {timeout})");

            var ret = await planeScanner.StartScan(
                planes,
                timeout,
                condition: PlaneConditionMatcher.IsValidPlane,
                token: this.tokenSource.Token);

            if (ret.result == PlaneScanResult.Found)
            {
                printLog($"Scan done: {ret.planesFound} planes found.");
            }
            else if (ret.result == PlaneScanResult.Timeout)
            {
                printLog($"Scan timeout: {ret.planesFound} planes found.");
            }
            else if (ret.result == PlaneScanResult.Error)
            {
                printLog($"Scan error: {ret.message}");
            }
            else if (ret.result == PlaneScanResult.Cancel)
            {
                printLog($"Scan cancel");
            }

            foreach (var plane in planeScanner.planeManager.planes)
            {
                plane.SetFlag(ARPlaneDebugFlag.ShowInfo);
            }
        }

        void Start()
        {
            var scanButton = button.gameObject.GetComponent<ScanButton>();
            button.OnClickAsObservable().Subscribe(async _ => {
                if(scanButton.Scan) {
                    scanButton.Switch();
                    await scan();
                    scanButton.Switch();
                } else {
                    this.tokenSource.Cancel();
                    this.tokenSource.Dispose();
                    this.tokenSource = new CancellationTokenSource();
                }
            }).AddTo(this);

            var session = ARServiceLocator.Instant.GetSession();
            session?.State.Subscribe(state =>
            {
                printLog($"Session: {state},{session.LostReason}");
            }).AddTo(this);

            ARServiceLocator.Instant.GetPlaneManager()?.DebugStatus.Subscribe(msg =>
            {
                printLog($"PlaneManager: {msg}");
            }).AddTo(this);
        }

        void OnDestroy()
        {
            this.tokenSource.Cancel();
            this.tokenSource.Dispose();
        }
    }
}
