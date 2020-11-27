using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    void Update()
    {
        float y = Time.deltaTime * 180;
        transform.Rotate(new Vector3(0, y, 0), Space.World);
    }
}
