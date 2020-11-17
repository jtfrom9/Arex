using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace Arex.ARFoundation
{
    public class ArexARFoundationServiceInstaller : MonoInstaller
    {
        [SerializeField] bool autoAddComponent = false;

        public override void InstallBindings()
        {
            var session = GameObject.FindObjectOfType<ARSession>();
            if (session != null)
            {
                var arexSession = session.gameObject.GetComponent<ArexARFoundationSession>();
                if (arexSession == null && autoAddComponent)
                {
                    arexSession = session.gameObject.AddComponent<ArexARFoundationSession>();
                }
                if (arexSession)
                {
                    Container.Bind<IARSession>().FromInstance(arexSession).AsSingle();
                }
            }

            var origin = GameObject.FindObjectOfType<ARSessionOrigin>();
            if (origin != null)
            {
                var arexARCamera = origin.gameObject.GetComponent<ArexARFoundationARCamera>();
                if(arexARCamera==null && autoAddComponent) {
                    arexARCamera = origin.gameObject.AddComponent<ArexARFoundationARCamera>();
                }
                if(arexARCamera) {
                    Container.Bind<IARCamera>().FromInstance(arexARCamera).AsSingle();
                }

                var arexPlaneManager = origin.gameObject.GetComponent<ArexARFoundationPlaneManager>();
                if (arexPlaneManager == null && autoAddComponent)
                {
                    arexPlaneManager = origin.gameObject.AddComponent<ArexARFoundationPlaneManager>();
                }
                if (arexPlaneManager)
                {
                    Container.Bind<IARPlaneManager>().FromInstance(arexPlaneManager).AsSingle();
                    Container.Bind<IARPlaneRaycastManager>().FromInstance(arexPlaneManager).AsSingle();
                }

                var arexOcclusionManager = origin.camera.gameObject.GetComponent<ArexARFoundationOcclusionManager>();
                if (arexOcclusionManager == null && autoAddComponent)
                {
                    arexOcclusionManager = origin.camera.gameObject.AddComponent<ArexARFoundationOcclusionManager>();
                }
                if (arexOcclusionManager)
                {
                    Container.Bind<IAROcclusionManager>().FromInstance(arexOcclusionManager).AsSingle();
                }
            }
        }
    }
}