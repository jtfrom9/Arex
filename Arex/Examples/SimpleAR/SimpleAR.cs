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
        [SerializeField] bool initOcclusion = false;

        List<string> logLines = new List<string>();
        [SerializeField] int lineOfLog = 5;

        // public void Init(IARSession session, IARPlaneManager planeManager)
        // {
        //     this.session = session;
        //     this.planeManager = planeManager;
        // }

        void Awake()
        {
            Assert.IsNotNull(toggleSession);
            Assert.IsNotNull(togglePlaneManager);
            Assert.IsNotNull(toggleVisualPlane);
            Assert.IsNotNull(textDebug);
        }


        void printLog(string msg)
        {
            logLines.Add($"{System.DateTime.Now.ToString("HH:mm:ss")} | {msg}");
            if(logLines.Count > 5)
                logLines.RemoveAt(0);
            textDebug.text = string.Join("\n", logLines);
        }

        void Start()
        {
            var session = ARServiceLocator.Instant.GetSession();
            var planeManager = ARServiceLocator.Instant.GetPlaneManager();
            var occlutionManager = ARServiceLocator.Instant.GetOcclusionManager();

            Debug.Log($"{session}");

            planeManager.Added.Subscribe(plane => {
                Debug.Log($"Added: {plane}");
                plane.visible = initVisiblePlane;
            }).AddTo(this);

            toggleSession.isOn = initSession;
            toggleSession.OnValueChangedAsObservable().Subscribe(v => {
                Debug.Log($"toggleSession: {v}");
                session.EnableSession = v;
            }).AddTo(this);

            togglePlaneManager.isOn = initPlaneSearch;
            togglePlaneManager.OnValueChangedAsObservable().Subscribe(v =>
            {
                Debug.Log($"togglePlaneManager: {v}");
                planeManager.EnableSearchPlanes = v;
            }).AddTo(this);

            toggleVisualPlane.isOn = initVisiblePlane;
            toggleVisualPlane.OnValueChangedAsObservable().Subscribe(v =>
            {
                Debug.Log($"toggleVisualPlane: {v}");
                foreach (var plane in planeManager.planes)
                {
                    plane.visible = v;
                }
            }).AddTo(this);

            if (toggleOcculusion != null)
            {
                toggleOcculusion.isOn = initOcclusion;
                toggleOcculusion.OnValueChangedAsObservable().Subscribe(v =>
                {
                    Debug.Log($"toggleOcculusion: {v}");
                    occlutionManager.EnableOcculusion = v;
                }).AddTo(this);
            }

            session.DebugStatus.Subscribe(msg => {
                printLog($"Session: {msg}");
            }).AddTo(this);

            planeManager.DebugStatus.Subscribe(msg =>
            {
                printLog($"PlaneManager: {msg}");
            }).AddTo(this);
        }
    }
}
