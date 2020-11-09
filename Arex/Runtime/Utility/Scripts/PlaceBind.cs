using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arex
{
    public class PlaceBind : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 relative;

        void Update()
        {
            transform.position = target.position +
                (target.right * relative.x) + (target.up * relative.y) + (target.forward * relative.z);
        }
    }
}