using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraPose : NetworkBehaviour
{
    [SerializeField] GameObject prefab = default;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($">> OnStartLocalPlayer");

        var go = Instantiate(prefab);
        go.transform.SetParent(transform);
    }
}
