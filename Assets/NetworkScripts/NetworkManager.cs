using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Menu Configuration")]
    [SerializeField] private GameObject _roomField;
    [SerializeField] private GameObject _nicknameField;
    [SerializeField] private GameObject _menuCanvas, _mainCanvas, _loadingPanel;
    [SerializeField] private GameObject _createRoomBtn, _joinRoomBtn, _roomPanel;
    [SerializeField] private TMP_Text _currentNickname, _currentPlayers, _currentRoom;
    [SerializeField] private GameObject _uiCamera;
    [SerializeField] private GameObject _eventSystem;
    [SerializeField] private Slider _loadingBar;


    [Header("Player")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Chat Configuration")]
    public TMP_InputField _chatInputField;
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
        PhotonNetwork.AutomaticallySyncScene = true;

        _menuCanvas.SetActive(true);
        _mainCanvas.SetActive(false);
        _loadingPanel.SetActive(false);
        _createRoomBtn.SetActive(false);
        _joinRoomBtn.SetActive(false);
        _roomField.SetActive(false);
        _nicknameField.SetActive(false);

        // if (_spawnPoint == null)
        //     _spawnPoint = GameObject.FindWithTag(_spawnPointTag).transform;
        if (_uiCamera == null)
            _uiCamera = GameObject.Find("CameraUI");

        // _defaultMessageColor = _currentPlayers.color;
        _defaultMessageColor = _currentPlayers.color;

        // if (_isInTesting)
        //     SpawnPlayer();
    }

    void Awake()
    {
        DontDestroyOnLoad(_menuCanvas);
        DontDestroyOnLoad(_mainCanvas);
        DontDestroyOnLoad(_loadingPanel);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void HandleCreateRoom()
    {
        _roomField.SetActive(!_roomField.activeInHierarchy);
        _createRoomBtn.SetActive(!_createRoomBtn.activeInHierarchy);
    }

    public void HandlePlay()
    {
        _createRoomBtn.SetActive(!_createRoomBtn.activeInHierarchy);
        _joinRoomBtn.SetActive(!_joinRoomBtn.activeInHierarchy);
        _nicknameField.SetActive(!_nicknameField.activeInHierarchy);
    }

    public void Join()
    {
        if (_nicknameField.GetComponent<TMP_InputField>().text.Trim() != "")
            Login();
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
        if (_roomField.GetComponent<TMP_InputField>().text.Trim() != "")
        {
            _menuCanvas.SetActive(false);
            _loadingPanel.SetActive(true);
            _eventSystem.SetActive(false);
            PhotonNetwork.JoinOrCreateRoom(_roomField.GetComponent<TMP_InputField>().text, new RoomOptions(), TypedLobby.Default);
        }
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
        if (IsPhotonViewInitialized())
        {
            photonView.RPC("ReceiveMessage", RpcTarget.All, message);
            Debug.Log(message);
            _chatInputField.text = "";
        }
        else
        {
            Debug.LogError("PhotonView is not initialized. Cannot send message.");
        }
    }

    [PunRPC]
    public void ReceiveMessage(string message)
    {
        chatMessages.Add(message);
        if (chatMessages.Count > 10) chatMessages.RemoveAt(0);
        UpdateChatDisplay();
    }

    private bool IsPhotonViewInitialized()
    {
        return PhotonView.Get(this) != null;
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
        //     if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        //     {
        //         bool playerExists = false;
        //         foreach (var player in PhotonNetwork.CurrentRoom.Players)
        //         {
        //             if (player.Value.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        //             {
        //                 playerExists = true;
        //                 break;
        //             }
        //         }

        //         if (!playerExists)
        PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position, Quaternion.identity, 0);
        // PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(-7.917f, 30.14f, 4.738f), Quaternion.identity, 0);
        //     }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        if (_roomField.activeInHierarchy)
        {
            CreateRoom();
            return;
        }
        // _createRoomBtn.SetActive(true);
        // _joinRoomBtn.SetActive(true);
        // _nicknameField.SetActive(true);
        Debug.Log("OnJoinedLobby");
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

    public void Load()
    {
        _uiCamera.SetActive(false);
        _loadingPanel.SetActive(false);
        _mainCanvas.SetActive(true);
        _roomPanel.SetActive(true);

        // if (_spawnPoint == null)
        //     _spawnPoint = GameObject.FindWithTag(_spawnPointTag).transform;

        UpdateRoomData();
        SpawnPlayer();

        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            SendMessageToChat($"Server: {PhotonNetwork.NickName} joined the room");

        // _loadingPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room!!");
        print($"Room name: {PhotonNetwork.CurrentRoom.Name}");
        print($"Players connected: {PhotonNetwork.CurrentRoom.PlayerCount}");

        _currentNickname.text = PhotonNetwork.NickName;
        _currentPlayers.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        _currentRoom.text = PhotonNetwork.CurrentRoom.Name;

        // // _lobbyPanel.SetActive(false);
        // _menuCanvas.SetActive(false);

        // UpdateRoomData();
        // SpawnPlayer();

        // register a callback event to be added when a new scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;

        // if (PhotonNetwork.IsMasterClient)
        // {
        LoadSceneWithProgress("Main");
        //     return;
        // }

        // SceneManager.sceneLoaded -= OnSceneLoaded;
        // Load();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // remove this callback to prevent this from being called in future scenes
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            _uiCamera.SetActive(false);
            _loadingPanel.SetActive(false);
            _mainCanvas.SetActive(true);
            _roomPanel.SetActive(true);

            if (_spawnPoint == null)
                _spawnPoint = GameObject.FindWithTag(_spawnPointTag).transform;

            UpdateRoomData();
            SpawnPlayer();

            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
                SendMessageToChat($"Server: {PhotonNetwork.NickName} joined the room");
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list updated!!");
        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);
        }
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

    public void LoadSceneWithProgress(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            _loadingBar.value = progress;

            if (asyncLoad.progress >= 0.9f)
            {
                _loadingBar.value = 1f;
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // private void SpawnPlayer()
    // {
    //     bool playerExists = false;
    //     foreach (var player in PhotonNetwork.CurrentRoom.Players)
    //     {
    //         if (player.Value.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
    //         {
    //             playerExists = true;
    //             break;
    //         }
    //     }

    //     if (!playerExists)
    //     {
    //         PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position, Quaternion.identity);
    //     }
    // }
}
