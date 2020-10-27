using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

namespace Arex.Examples
{
    public class DebugPanel : MonoBehaviour
    {
        public Text debugText;
        string textCache;

        List<string> logLines = new List<string>();
        [SerializeField] int lineOfLog = 5;

        // void Start()
        // {
        //     this.UpdateAsObservable()
        //         .Where(_ => !string.IsNullOrEmpty(textCache))
        //         .ThrottleFirst(System.TimeSpan.FromMilliseconds(500))
        //         .Subscribe(_ =>
        //         {
        //             debugText.text = textCache;
        //             Debug.Log($"hoge: {textCache}");
        //         }).AddTo(this);
        // }

        public void PrintLog(string msg)
        {
            logLines.Add($"{System.DateTime.Now.ToString("HH:mm:ss")} | {msg}");
            if (logLines.Count > 5)
                logLines.RemoveAt(0);
            textCache = string.Join("\n", logLines);
            debugText.text = textCache;
            Debug.Log(msg);
        }
    }
}

