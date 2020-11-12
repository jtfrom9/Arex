using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arex.Visual
{
    public class WavingQuad : MonoBehaviour
    {
        [SerializeField] GameObject quad = default;

        public float offsetY {
            get => quad.transform.position.y;
            set
            {
                var pos = quad.transform.position;
                quad.transform.position = new Vector3(pos.x, value, pos.z);
            }
        }
    }
}