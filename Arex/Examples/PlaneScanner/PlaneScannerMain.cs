using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Arex.Examples
{
    public class PlaneScannerMain : MonoBehaviour
    {
        public PlaneScanner planeScanner;

        public Button buttonStartScan;
        public Text textDebug;

        List<string> logLines = new List<string>();
        [SerializeField] int lineOfLog = 5;

        void printLog(string msg)
        {
            Debug.Log(msg);

            logLines.Add($"{System.DateTime.Now.ToString("HH:mm:ss")} | {msg}");
            if (logLines.Count > 5)
                logLines.RemoveAt(0);
            textDebug.text = string.Join("\n", logLines);
        }

        void Start()
        {
            buttonStartScan.OnClickAsObservable().Subscribe(async _ => {
                buttonStartScan.interactable = false;

                int planes = 3;
                int timeout = 10;
                printLog($"Start Scan Plane (planes: {planes}, timeout: {timeout})");

                var ret = await planeScanner.StartScan(
                    planes,
                    timeout,
                    condition: PlaneConditionMatcher.IsValidPlane,
                    token: this.GetCancellationTokenOnDestroy());

                if (ret.result == PlaneScanResult.Found)
                {
                    printLog($"Scan done: {ret.planesFound} planes found.");
                } else if(ret.result==PlaneScanResult.Timeout)
                {
                    printLog($"Scan timeout: {ret.planesFound} planes found.");
                } else if(ret.result==PlaneScanResult.Error) {
                    printLog($"Scan error: {ret.message}");
                } else if (ret.result == PlaneScanResult.Cancel) {
                    printLog($"Scan cancel");
                }

                foreach(var plane in planeScanner.planeManager.planes) {
                    plane.SetDebug(ARPlaneDebugFlag.ShowInfo);
                }
                buttonStartScan.interactable = true;
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
    }
}
