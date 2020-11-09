using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using Zenject;
using UniRx;

namespace Arex.ARFoundation
{
    public class ARFoundationDirectionAlignment : MonoBehaviour
    {
        ILocationDataProvider locationDataProvider;
        ARSessionOrigin origin;

        public enum UpdateType {
            Once = 0,
            Notified,
            Explicit
        }

        [SerializeField] UpdateType updateType = UpdateType.Once;

        [Inject]
        void Inject(ILocationDataProvider locationDataProvider)
        {
            this.locationDataProvider = locationDataProvider;
        }

        void Awake()
        {
            origin = GetComponent<ARSessionOrigin>();
            Assert.IsNotNull(origin);
        }

        void align(float direction)
        {
            var rotY = origin.camera.transform.rotation.eulerAngles.y - direction;
            origin.MakeContentAppearAt(origin.transform,
                rotation: Quaternion.Euler(0, rotY, 0));
        }

        void Start()
        {
            switch(updateType) {
                case UpdateType.Once:
                    this.locationDataProvider.Current.SkipLatestValueOnSubscribe().Take(1)
                        .Subscribe(location =>
                        {
                            align(location.Direction);
                        }).AddTo(this);
                    break;
                case UpdateType.Notified:
                    this.locationDataProvider.Current.SkipLatestValueOnSubscribe()
                        .Subscribe(location =>
                        {
                            align(location.Direction);
                        }).AddTo(this);
                    break;
                default:
                    break;
            }
        }

        public void Align()
        {
            align(this.locationDataProvider.Current.Value.Direction);
        }
    }
}
