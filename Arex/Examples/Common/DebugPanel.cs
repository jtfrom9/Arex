using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Arex.Examples
{
    public class DebugPanel : MonoBehaviour
    {
        public Text debugText;

        List<string> logLines = new List<string>();
        [SerializeField] int lineOfLog = 5;

        public void PrintLog(string msg)
        {
            logLines.Add($"{System.DateTime.Now.ToString("HH:mm:ss")} | {msg}");
            if (logLines.Count > 5)
                logLines.RemoveAt(0);
            debugText.text = string.Join("\n", logLines);
        }
    }
}
