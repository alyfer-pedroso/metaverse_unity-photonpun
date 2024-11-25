using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
// using UnityEditor.UIElements;

public class Conn : MonoBehaviourPunCallbacks
{
    [Header("System Configuration")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _colorPanel;
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _roomPanel;
    [SerializeField] private GameObject _colorBtn;
    [SerializeField] private GameObject _cameraUI;
    [SerializeField] private TMP_InputField _nicknameInput, _roomnameInput;
    [SerializeField] private TMP_Text _currentNickname, _currentPlayers, _currentRoom;
    public bool _isLocal = false;


    [Header("Player")]
    [SerializeField] private GameObject _playerPrefab;
    public CUIColorPicker _colorPicker;
    public string _localPlayerNickname = "";

    [Header("Chat Configuration")]
    public TMP_InputField _chatInputField;
    [SerializeField] private TMP_Text _chatDisplay;
    private Color _defaultMessageColor;

    [Header("Spawn Configuration")]
    [SerializeField] private string _spawnPointTag;
    public Transform _spawnPoint;

    private readonly List<string> chatMessages = new();


    // Start is called before the first frame update
    void Start()
    {
        _loginPanel.SetActive(true);
        _colorPanel.SetActive(false);
        _lobbyPanel.SetActive(false);
        _colorBtn.SetActive(false);
        _roomPanel.SetActive(false);
        _cameraUI.SetActive(true);

        if (_spawnPoint == null)
            _spawnPoint = GameObject.FindWithTag(_spawnPointTag).transform;

        _defaultMessageColor = _chatInputField.textComponent.color;

        // if (_colorPicker == null)
        //     _colorPicker = FindObjectOfType<CUIColorPicker>().GetComponent<CUIColorPicker>();
    }

    public void Login()
    {
        if (!_isLocal)
        {
            PhotonNetwork.NickName = _nicknameInput.text;
            PhotonNetwork.ConnectUsingSettings();
        }
        _localPlayerNickname = _nicknameInput.text;
        _loginPanel.SetActive(false);
        _colorPanel.SetActive(true);
        _colorBtn.SetActive(_isLocal);
    }

    public void CreateRoom()
    {
        if (!_isLocal)
        {
            PhotonNetwork.JoinOrCreateRoom(_roomnameInput.text, new RoomOptions(), TypedLobby.Default);
            return;
        }
        PlayerToWorld();
    }


    public void SendMessageToChat(string message)
    {
        SendMessageRPC(message);
    }

    public void SendInputMessageToChat()
    {
        if (_chatInputField != null && !string.IsNullOrEmpty(_chatInputField.text))
        {
            // _chatInputField.textComponent.color = _defaultMessageColor;
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
        if (_isLocal)
        {
            Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
            return;
        }

        PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position, Quaternion.identity, 0);
    }

    public void ToRoomPanel()
    {
        _colorPanel.SetActive(false);
        _lobbyPanel.SetActive(true);
    }

    public void PlayerToWorld()
    {
        if (!_isLocal)
        {
            Debug.Log("Joined room!!");
            print($"Room name: {PhotonNetwork.CurrentRoom.Name}");
            print($"Players connected: {PhotonNetwork.CurrentRoom.PlayerCount}");

            _currentNickname.text = PhotonNetwork.NickName;
            _currentPlayers.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
            _currentRoom.text = PhotonNetwork.CurrentRoom.Name;
            _roomPanel.SetActive(true);
        }

        _lobbyPanel.SetActive(false);
        _cameraUI.SetActive(false);

        SpawnPlayer();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!!");
        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby!!");
        _colorBtn.SetActive(true);
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
        PlayerToWorld();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomData();
        // _chatInputField.textComponent.color = Color.yellow;
        SendMessageToChat($"{newPlayer.NickName} joined the room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomData();
        // _chatInputField.textComponent.color = Color.red;
        SendMessageToChat($"{otherPlayer.NickName} left the room");
    }
}
