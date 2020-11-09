using UnityEngine;
using Zenject;

namespace Arex
{
    [RequireComponent(typeof(SimpleLocationProvider))]
    public class SimpleLocationProviderInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ILocationDataProvider>()
                .FromInstance(GetComponent<SimpleLocationProvider>())
                .AsSingle();
        }
    }
}