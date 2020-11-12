using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arex.Visual
{
    public class WavingQuadDemo : MonoBehaviour
    {
        [SerializeField] WavingQuad wavingQuad = default;

        float y = 0;
        float diff = 0.01f;

        void Start()
        {
            InvokeRepeating("modifyOffset", 1, 0.01f);
        }

        void modifyOffset()
        {
            y += diff;
            Debug.Log($"y={y}");

            if(y > 0.5f) {
                diff = -diff;
            } else if(y < -0.5f) {
                diff = -diff;
            }
            wavingQuad.offsetY = y;
        }
    }
}
