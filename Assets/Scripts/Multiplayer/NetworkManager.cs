using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Collections.Generic;
using System;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [SerializeField] private TMP_InputField _roomNameInputField;
    [SerializeField] private TMP_InputField PlayerNameInput;
    [SerializeField] private int _maxPlayers;
    [SerializeField] private TMP_Text _errorText;
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private GameObject _roomListItemPrefab;
    [SerializeField] private Transform _roomListContent;

    private bool isConnecting = false;
    private GameObject _playerListItemPrefab;
    private Transform _playerListContent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeSetting();
    }

    private void InitializeSetting()
    {
        PlayerNameInput.text = "Player " + UnityEngine.Random.Range(1000, 10000);
    }

    private void InitializePhoton()
    {
        PhotonNetwork.LogLevel = PunLogLevel.Full;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.0";

        // КРИТИЧЕСКИ ВАЖНЫЕ НАСТРОЙКИ ДЛЯ ПРЕДОТВРАЩЕНИЯ ТАЙМАУТА
        PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;
        PhotonNetwork.PhotonServerSettings.AppSettings.EnableProtocolFallback = true;

        // Настройки Photon Peer для стабильности
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.TimePingInterval = 2000;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 15000;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.SentCountAllowance = 15;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.InitialResendTimeMax = 5;

        // Отключаем пакетную отправку для лучшей стабильности
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSendingOnlyAcks = false;


        LogNetworkInfo();
        Debug.Log("Connecting to Photon...");
        isConnecting = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    #region OverrideMethods

    public override void OnConnectedToMaster()
    {
        Debug.Log("✓ Connected to Master Server");
        Debug.Log($"Region: {PhotonNetwork.CloudRegion}, Ping: {PhotonNetwork.GetPing()}ms");
        Debug.Log($"Server: {PhotonNetwork.Server}, CloudRegion: {PhotonNetwork.CloudRegion}");

        // Принудительно укажите регион если нужно
        // PhotonNetwork.ConnectToRegion("eu"); // или "us", "asia" и т.д.

        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        isConnecting = false;
        MenuManager.Instance.OpenMenu("TitleMenu");
        PhotonNetwork.NickName = $"Player_" + Guid.NewGuid();
        Debug.Log("✓ Connected to Lobby");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"✓ SUCCESS: Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");

        Player[] players = PhotonNetwork.PlayerList;
        foreach (var player in players)
        {
            Instantiate(_playerListItemPrefab, _playerListContent)
            .GetComponent<PlayerListItem>()
            .SetUp(player);
        }

        MenuManager.Instance.OpenMenu("RoomMenu");
        _roomNameText.text = PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create room failed: {returnCode} - {message}");
        _errorText.text = $"Room Creation Failed: {message}";
        MenuManager.Instance.OpenMenu("ErrorMenu");
        isConnecting = false;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join room failed: {returnCode} - {message}");
        _errorText.text = $"Join Room Failed: {message}";
        MenuManager.Instance.OpenMenu("ErrorMenu");
        isConnecting = false;
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("TitleMenu");
        Debug.Log("Left room");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected: {cause}");

        if (isConnecting)
        {
            _errorText.text = $"Connection failed: {cause}";
            MenuManager.Instance.OpenMenu("ErrorMenu");
        }

        isConnecting = false;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in _roomListContent)
        {
            Destroy(trans.gameObject);
        }

        foreach (var room in roomList)
        {
            if (!room.RemovedFromList && room.IsOpen && room.IsVisible)
            {
                GameObject clone = Instantiate(_roomListItemPrefab, _roomListContent);
                clone.GetComponent<RoomListItem>().SetUp(room);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(_playerListItemPrefab, _playerListContent)
             .GetComponent<PlayerListItem>()
             .SetUp(newPlayer);
    }

    #endregion

    #region PublicNetwork

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(_roomNameInputField.text))
        {
            _errorText.text = "Please enter room name";
            MenuManager.Instance.OpenMenu("ErrorMenu");
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            _errorText.text = "Not connected to Photon";
            MenuManager.Instance.OpenMenu("ErrorMenu");
            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = (byte)_maxPlayers,
            PlayerTtl = 30000,
            EmptyRoomTtl = 10000,
            PublishUserId = true,
            CleanupCacheOnLeave = false
        };

        Debug.Log($"Creating room: {_roomNameInputField.text}");
        isConnecting = true;
        PhotonNetwork.CreateRoom(_roomNameInputField.text, roomOptions);

        MenuManager.Instance.OpenMenu("LoadingMenu");
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            MenuManager.Instance.OpenMenu("LoadingMenu");
        }
    }

    public void JoinRoom(RoomInfo info)
    {
        if (!PhotonNetwork.IsConnected)
        {
            _errorText.text = "Not connected to Photon";
            MenuManager.Instance.OpenMenu("ErrorMenu");
            return;
        }

        if (info.PlayerCount >= info.MaxPlayers)
        {
            _errorText.text = "Room is full";
            MenuManager.Instance.OpenMenu("ErrorMenu");
            return;
        }


        Debug.Log($"Joining room: {info.Name}");
        isConnecting = true;
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("LoadingMenu");
    }

    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
        {
            isConnecting = true;
            PhotonNetwork.JoinRandomRoom();
            MenuManager.Instance.OpenMenu("LoadingMenu");
        }
    }

    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            InitializePhoton();
            MenuManager.Instance.OpenMenu("LoadingMenu");
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    #endregion

    void Update()
    {
        // Мониторинг состояния
        if (Time.frameCount % 200 == 0)
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log($"State: {PhotonNetwork.NetworkClientState}, Ping: {PhotonNetwork.GetPing()}ms, InRoom: {PhotonNetwork.InRoom}");
            }
        }
    }


    public void Reconnect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            InitializePhoton();
        }
    }
    void LogNetworkInfo()
    {
        Debug.Log($"=== NETWORK DIAGNOSTICS ===");
        Debug.Log($"Ping: {PhotonNetwork.GetPing()}ms");
        Debug.Log($"Region: {PhotonNetwork.CloudRegion}");
        Debug.Log($"Server: {PhotonNetwork.ServerAddress}");
        Debug.Log($"State: {PhotonNetwork.NetworkClientState}");
        Debug.Log($"IsConnected: {PhotonNetwork.IsConnected}");
        Debug.Log($"InLobby: {PhotonNetwork.InLobby}");
        Debug.Log($"InRoom: {PhotonNetwork.InRoom}");
    }
}