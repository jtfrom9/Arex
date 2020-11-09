using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace Arex.ARFoundation
{
    public class DirectionAlign : MonoBehaviour
    {
        [SerializeField] Text logText = default;

        ARSessionOrigin origin;
        [Inject] ILocationDataProvider locationDataProvider;

        void Start()
        {
            this.origin = GameObject.FindObjectOfType<ARSessionOrigin>();
        }

        void Update()
        {
            var sb = new StringBuilder(1024);
            sb.AppendFormat("Origin: {0}, {1}",
                this.origin.transform.position.ToString(),
                this.origin.transform.rotation.eulerAngles.ToString()).AppendLine();
            sb.AppendFormat("Camera (global): {0}, {1}",
                this.origin.camera.transform.position.ToString(),
                this.origin.camera.transform.rotation.eulerAngles.ToString()).AppendLine();
            sb.AppendFormat("Camera (local): {0}, {1}",
                this.origin.camera.transform.localPosition.ToString(),
                this.origin.camera.transform.localRotation.eulerAngles.ToString()).AppendLine();
            logText.text = sb.ToString();
        }
    }
}