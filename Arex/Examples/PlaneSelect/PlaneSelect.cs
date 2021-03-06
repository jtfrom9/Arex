﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Zenject;
using Arex;

namespace Arex.Examples
{
    public class PlaneSelect : MonoBehaviour
    {
        [Inject] IARPlaneManager planeManager = default;
        [Inject] IARPlaneRaycastManager raycastManager = default;

        [SerializeField] GameObject prefab = default;

        GameObject target;

        void Start()
        {
            this.planeManager.EnableSearchPlanes = true;

            var center = new Vector2 { x = Screen.width / 2, y = Screen.height / 2 };

            this.UpdateAsObservable().Subscribe(_ => {
                RaycastHit hit;
                if (this.raycastManager.Raycast(center, out hit))
                {
                    Debug.Log($"rot = {hit.pose.rotation.eulerAngles}");
                    if (target == null)
                    {
                        target = Instantiate(prefab, hit.pose.position, hit.pose.rotation);
                        var wavingQuad = target.GetComponent<Arex.Visual.WavingQuad>();
                        if (wavingQuad != null)
                        {
                            wavingQuad.offsetY += 0.1f;
                            wavingQuad.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        }
                    }
                    else
                    {
                        target.transform.position = hit.pose.position;
                        target.transform.rotation = hit.pose.rotation;
                    }
                }
            }).AddTo(this);
        }
    }
}