using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Conn : MonoBehaviourPunCallbacks
{
    [Header("Configs")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _roomPanel;
    [SerializeField] private GameObject _joinRoomBtn;
    [SerializeField] private TMP_InputField _nicknameInput, _roomnameInput;
    [SerializeField] private TMP_Text _currentNickname, _currentPlayers, _currentRoom;

    [Header("Player")]
    [SerializeField] private GameObject _playerPrefab;


    // Start is called before the first frame update
    void Start()
    {
        _loginPanel.SetActive(true);
        _lobbyPanel.SetActive(false);
        _joinRoomBtn.SetActive(false);
        _roomPanel.SetActive(false);
    }

    public void Login()
    {
        PhotonNetwork.NickName = _nicknameInput.text;
        PhotonNetwork.ConnectUsingSettings();
        _loginPanel.SetActive(false);
        _lobbyPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(_roomnameInput.text, new RoomOptions(), TypedLobby.Default);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateRoomData()
    {
        _currentPlayers.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!!");
        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby!!");
        _joinRoomBtn.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Lost connection!!");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room!!");
        // PhotonNetwork.CreateRoom(null, new RoomOptions());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room!!");
        print($"Room name: {PhotonNetwork.CurrentRoom.Name}");
        print($"Players connected: {PhotonNetwork.CurrentRoom.PlayerCount}");

        _currentNickname.text = PhotonNetwork.NickName;
        _currentPlayers.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        _currentRoom.text = PhotonNetwork.CurrentRoom.Name;

        _lobbyPanel.SetActive(false);
        _roomPanel.SetActive(true);

        PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(0, 0, Random.Range(1, 8)), Quaternion.identity, 0);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomData();
        Debug.Log($"{newPlayer.NickName} joined the room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomData();
        Debug.Log($"{otherPlayer.NickName} left the room");
    }
}
