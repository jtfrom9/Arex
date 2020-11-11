using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Arex.Examples
{
    public class PlaneScannerMain : MonoBehaviour
    {
        public PlaneScanner planeScanner;

        public Button button;
        public DebugPanel debugPanel;

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        [Inject] IARSession session;
        [Inject] IARPlaneManager planeManager;

        void printLog(string msg)
        {
            debugPanel.PrintLog(msg);
        }

        async UniTask scan()
        {
            int planes = 3;
            int timeout = 10;
            printLog($"Start Scan Plane (planes: {planes}, timeout: {timeout})");

            var ret = await planeScanner.Scan(
                planes,
                timeout,
                this.tokenSource.Token);

            if (ret.result == PlaneScanResult.Found)
            {
                printLog($"Scan done: {ret.planes.Count} planes found.");
            }
            else if (ret.result == PlaneScanResult.Timeout)
            {
                printLog($"Scan timeout: {ret.planes.Count} planes found.");
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

            session.State.Subscribe(state =>
            {
                printLog($"Session: {state},{session.LostReason}");
            }).AddTo(this);

            planeManager.DebugStatus.Subscribe(msg =>
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
