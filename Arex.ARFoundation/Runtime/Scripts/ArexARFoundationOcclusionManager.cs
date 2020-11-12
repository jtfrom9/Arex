using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(AROcclusionManager))]
    public class ArexARFoundationOcclusionManager : MonoBehaviour, IAROcclusionManager
    {
        AROcclusionManager occlusionManager;

        void Awake()
        {
            occlusionManager = GetComponent<AROcclusionManager>();
        }

        public bool EnableOcculusion
        {
            get => occlusionManager.enabled;
            set => occlusionManager.enabled = value;
        }
    }
}
