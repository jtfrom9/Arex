using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Arex.Examples
{
    public class ScanButton : MonoBehaviour
    {
        public Text text;

        void Start()
        {
            Switch();
        }

        bool _scan = false;
        public void  Switch() {
            if (!_scan)
            {
                text.text = "Scan";
            }
            else
            {
                text.text = "Cancel";
            }
            _scan = !_scan;
        }
        public bool Scan { get => _scan; }
    }
}
