using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraServer : NetworkManager
{
    [SerializeField] Camera _camera = default;
    [SerializeField] GameObject cameraPosePrefab = default;
    GameObject cameraPose = null;

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log($">> OnServerConnect");
        cameraPose = Instantiate(cameraPosePrefab);
        NetworkServer.AddPlayerForConnection(conn, cameraPose);
    }

    // public override void OnClientConnect(NetworkConnection conn)
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log($">> OnServerAddPlayer. {conn.identity}");
    }

    void Update()
    {
        if(cameraPose) {
            cameraPose.transform.position = _camera.transform.position;
            cameraPose.transform.rotation = _camera.transform.rotation;
        }
    }
}
