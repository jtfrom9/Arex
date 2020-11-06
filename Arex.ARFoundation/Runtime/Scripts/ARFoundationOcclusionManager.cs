using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(AROcclusionManager))]
    public class ARFoundationOcclusionManager : MonoBehaviour, IAROcclusionManager
    {
        AROcclusionManager occlusionManager;

        void Awake()
        {
            ARServiceLocator.Instant.Register(this);
            occlusionManager = GetComponent<AROcclusionManager>();
        }

        public bool EnableOcculusion
        {
            get => occlusionManager.enabled;
            set => occlusionManager.enabled = value;
        }
    }
}
