using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace Arex.ARFoundation
{
    public class ARFoundationServiceInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var session = GameObject.FindObjectOfType<ARSession>();
            if(session!=null) {
                var arexSession = session.gameObject.GetComponent<ARFoundationSession>();
                if (arexSession != null)
                {
                    Container.Bind<IARSession>().FromInstance(arexSession).AsSingle();
                }
            }

            var origin = GameObject.FindObjectOfType<ARSessionOrigin>();
            if(origin!=null) {
                var arexPlaneManager = origin.gameObject.GetComponent<ARFoundationPlaneManager>();
                if (arexPlaneManager != null)
                {
                    Container.Bind<IARPlaneManager>().FromInstance(arexPlaneManager).AsSingle();
                    Container.Bind<IARPlaneRaycastManager>().FromInstance(arexPlaneManager).AsSingle();
                }

                var arexOcclusionManager = origin.camera.gameObject.GetComponent<ARFoundationOcclusionManager>();
                if(arexOcclusionManager!=null) {
                    Container.Bind<IAROcclusionManager>().FromInstance(arexOcclusionManager).AsSingle();
                }
            }
        }
    }
}