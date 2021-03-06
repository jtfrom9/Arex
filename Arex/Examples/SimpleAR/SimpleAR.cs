﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UniRx;
using Zenject;

namespace Arex.Examples
{
    public class SimpleAR : MonoBehaviour
    {
        public Toggle toggleSession;
        public Toggle togglePlaneManager;
        public Toggle toggleVisualPlane;
        public Toggle toggleOcculusion;
        public DebugPanel debugPanel;

        [SerializeField] bool initSession = true;
        [SerializeField] bool initPlaneSearch = false;
        [SerializeField] bool initVisiblePlane = true;
        [SerializeField] bool initOcclusion = false;

        [Inject] IARSession session = default;
        [Inject] IARPlaneManager planeManager = default;
        [Inject] IAROcclusionManager occlusionManager = default;

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
            Assert.IsNotNull(debugPanel);
        }

        void printLog(string msg)
        {
            debugPanel.PrintLog(msg);
        }

        void Start()
        {
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
                    this.occlusionManager.EnableOcculusion = v;
                }).AddTo(this);
            }

            session.State.Subscribe(s => {
                printLog($"Session: {s.ToString()}, {session.LostReason}");
            }).AddTo(this);

            planeManager.DebugStatus.Subscribe(msg =>
            {
                printLog($"PlaneManager: {msg}");
            }).AddTo(this);
        }
    }
}
