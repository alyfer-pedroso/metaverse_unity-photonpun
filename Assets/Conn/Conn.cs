using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Conn : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) PhotonNetwork.Disconnect();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!!");
        PhotonNetwork.JoinLobby();
        // PhotonNetwork.JoinRandomRoom();
        // base.OnConnectedToMaster();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby!!");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Lost connection!!");
        // base.OnDisconnected(cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room!!");
        PhotonNetwork.CreateRoom(null, new RoomOptions());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room!!");
    }
}
