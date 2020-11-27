using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CubeNetworkManager : NetworkManager
{
    [SerializeField] GameObject prefab;

    GameObject cube = null;

    public override void OnServerConnect(NetworkConnection conn)
    {
        // base.OnClientConnect(conn);

        Debug.Log($">>> OnClientConnect: players {numPlayers}");
        var go = Instantiate(prefab);
        go.AddComponent<Rotate>();
        // NetworkServer.Spawn(go, conn);
        NetworkServer.AddPlayerForConnection(conn, go);
        cube = go;
    }

    public void ToggleRotate()
    {
        if (cube)
        {
            var rot = cube.GetComponent<Rotate>();
            rot.enabled = !rot.enabled;
        }
    }
}
