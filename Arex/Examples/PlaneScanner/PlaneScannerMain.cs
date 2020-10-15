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

                printLog("Start Scan Plane");

                var ret = await planeScanner.StartScan(planes: 3,
                    timeout: 30,
                    condition: PlaneConditionMatcher.IsValidPlane,
                    token: this.GetCancellationTokenOnDestroy());

                printLog($"DONE {ret.result.ToString()}, planes: {ret.planesFound}");

                foreach(var plane in planeScanner.planeManager.planes) {
                    plane.SetDebug(ARPlaneDebugFlag.ShowInfo);
                }

                buttonStartScan.interactable = true;
            }).AddTo(this);

            ARServiceLocator.Instant.GetSession()?.DebugStatus.Subscribe(msg =>
            {
                printLog($"Session: {msg}");
            }).AddTo(this);

            ARServiceLocator.Instant.GetPlaneManager()?.DebugStatus.Subscribe(msg =>
            {
                printLog($"PlaneManager: {msg}");
            }).AddTo(this);
        }
    }
}
