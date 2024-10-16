using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Menu Configuration")]
    [SerializeField] private GameObject _roomField;
    [SerializeField] private GameObject _nicknameField;
    [SerializeField] private GameObject _createRoomBtn, _joinRoomBtn;
    [SerializeField] private TMP_Text _currentNickname, _currentPlayers, _currentRoom;


    [Header("Player")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Chat Configuration")]
    [SerializeField] private TMP_InputField _chatInputField;
    [SerializeField] private TMP_Text _chatDisplay;
    private Color _defaultMessageColor;
    private readonly List<string> chatMessages = new();


    [Header("Spawn Configuration")]
    [SerializeField] private string _spawnPointTag;
    public Transform _spawnPoint;

    [Header("Testing")]
    public bool _isInTesting = false;



    // Start is called before the first frame update
    void Start()
    {
        _createRoomBtn.SetActive(false);
        _joinRoomBtn.SetActive(false);
        _roomField.SetActive(false);
        _nicknameField.SetActive(false);

        if (_spawnPoint == null)
            _spawnPoint = GameObject.FindWithTag(_spawnPointTag).transform;

        _defaultMessageColor = _currentPlayers.color;

        if (_isInTesting)
            SpawnPlayer();
    }

    public void HandleCreateRoom()
    {
        _roomField.SetActive(!_roomField.activeInHierarchy);
        _createRoomBtn.SetActive(!_createRoomBtn.activeInHierarchy);
    }

    public void Login()
    {
        PhotonNetwork.NickName = _nicknameField.GetComponent<TMP_InputField>().text;
        PhotonNetwork.ConnectUsingSettings();
        // _loginPanel.SetActive(false);
        // _lobbyPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(_roomField.GetComponent<TMP_InputField>().text, new RoomOptions(), TypedLobby.Default);
    }


    public void SendMessageToChat(string message)
    {
        SendMessageRPC(message);
    }

    public void SendInputMessageToChat()
    {
        if (_chatInputField != null && !string.IsNullOrEmpty(_chatInputField.text))
        {
            _currentPlayers.color = _defaultMessageColor;
            SendMessageToChat($"{PhotonNetwork.NickName}: {_chatInputField.text}");
        }
    }

    public void SendMessageRPC(string message)
    {
        photonView.RPC("ReceiveMessage", RpcTarget.All, message);
        _chatInputField.text = "";
    }

    [PunRPC]
    public void ReceiveMessage(string message)
    {
        chatMessages.Add(message);
        if (chatMessages.Count > 10) chatMessages.RemoveAt(0);
        UpdateChatDisplay();
    }

    public void UpdateChatDisplay()
    {
        _chatDisplay.text = string.Join("\n", chatMessages);
    }

    public void UpdateRoomData()
    {
        _currentPlayers.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
    }

    public void SpawnPlayer()
    {
        if (_isInTesting)
        {
            Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
            return;
        }

        PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position, Quaternion.identity, 0);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!!");
        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby!!");
        _createRoomBtn.SetActive(true);
        _joinRoomBtn.SetActive(true);
        _nicknameField.SetActive(true);
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

        // _lobbyPanel.SetActive(false);
        // _roomPanel.SetActive(true);

        UpdateRoomData();
        SpawnPlayer();

        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            SendMessageToChat($"Server: {PhotonNetwork.NickName} joined the room");
    }

    public override void OnLeftRoom()
    {
        UpdateRoomData();
        SendMessageToChat($"Server: {PhotonNetwork.NickName} left the room");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomData();
        if (PhotonNetwork.IsMasterClient)
            SendMessageToChat($"Server: {newPlayer.NickName} joined the room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomData();
        if (PhotonNetwork.IsMasterClient)
            SendMessageToChat($"Server: {otherPlayer.NickName} left the room");
    }
}
