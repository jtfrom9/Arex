using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UniRx;

namespace Arex.ARFoundation
{
    public class SimpleAR : MonoBehaviour
    {
        public Toggle toggleSession;
        public Toggle togglePlaneManager;
        public Toggle toggleVisualPlane;
        public Toggle toggleOcculusion;
        public Text textDebug;

        [SerializeField] bool initSession = true;
        [SerializeField] bool initPlaneSearch = false;
        [SerializeField] bool initVisiblePlane = true;

        IAREnvironment arEnv;

        void Awake()
        {
            Assert.IsNotNull(toggleSession);
            Assert.IsNotNull(togglePlaneManager);
            Assert.IsNotNull(toggleVisualPlane);
            Assert.IsNotNull(textDebug);

            arEnv = FindObjectOfType<ARFoundationPlaneManager>() as IAREnvironment;
            Debug.Log($"arEnv = {arEnv}");
        }

        void Start()
        {
            arEnv.Added.Subscribe(plane => {
                Debug.Log($"Added: {plane}");
                plane.visible = initVisiblePlane;
            }).AddTo(this);

            toggleSession.isOn = initSession;
            toggleSession.OnValueChangedAsObservable().Subscribe(v => {
                Debug.Log($"toggleSession: {v}");
                arEnv.EnableSession = v;
            }).AddTo(this);

            togglePlaneManager.isOn = initPlaneSearch;
            togglePlaneManager.OnValueChangedAsObservable().Subscribe(v =>
            {
                Debug.Log($"togglePlaneManager: {v}");
                arEnv.EnableSearchPlanes = v;
            }).AddTo(this);

            toggleVisualPlane.isOn = initVisiblePlane;
            toggleVisualPlane.OnValueChangedAsObservable().Subscribe(v =>
            {
                Debug.Log($"toggleVisualPlane: {v}");
                foreach (var plane in arEnv.planes)
                {
                    plane.visible = v;
                }
            }).AddTo(this);

            toggleOcculusion?.OnValueChangedAsObservable().Subscribe(v =>
            {
                Debug.Log($"toggleOcculusion: {v}");
                arEnv.EnableOcculusion = v;
            }).AddTo(this);

            arEnv.DebugStatus.Subscribe(msg => {
                textDebug.text = msg;
            }).AddTo(this);
        }
    }
}
