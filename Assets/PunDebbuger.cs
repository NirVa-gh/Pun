using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;

public class PunDebugger : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Включите подробное логирование
        Debug.Log("=== PHOTON DEBUG START ===");

        PhotonNetwork.LogLevel = PunLogLevel.Full;
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.GameVersion = "1.0";

        // Принудительные настройки
        PhotonNetwork.PhotonServerSettings.AppSettings.Protocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;
        PhotonNetwork.PhotonServerSettings.AppSettings.EnableProtocolFallback = true;

        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnected()
    {
        Debug.Log("OnConnected() - Basic connection established");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✓ Connected to Master Server");
        Debug.Log($"Region: {PhotonNetwork.CloudRegion}");
        Debug.Log($"Ping: {PhotonNetwork.GetPing()} ms");

        StartCoroutine(TestConnection());
    }

    IEnumerator TestConnection()
    {
        Debug.Log("Waiting 2 seconds before room creation...");
        yield return new WaitForSeconds(2f);

        // Пробуем разные методы
        Debug.Log("Attempting to join random room...");
        bool success = PhotonNetwork.JoinRandomRoom();

        if (!success)
        {
            Debug.Log("No rooms found, creating new room...");
            CreateTestRoom();
        }
    }

    void CreateTestRoom()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true,
            EmptyRoomTtl = 0, // Комната сразу удаляется если пустая
            PlayerTtl = 30000,
            PublishUserId = true
        };

        TypedLobby typedLobby = new TypedLobby("testLobby", LobbyType.Default);

        Debug.Log("Creating room...");
        PhotonNetwork.CreateRoom($"TestRoom_{Random.Range(1000, 9999)}", options, typedLobby);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("✓ SUCCESS: Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log($"Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"Join random failed: {returnCode} - {message}");
        CreateTestRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create room failed: {returnCode} - {message}");

        // Пробуем с другими настройками
        if (returnCode == 32758) // Room already exists
        {
            CreateTestRoom();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected: {cause}");

        // Авто-реконнект
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            Debug.Log("Attempting reconnect in 3 seconds...");
            Invoke("Reconnect", 3f);
        }
    }

    void Reconnect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void Update()
    {
        // Мониторинг состояния
        if (Time.frameCount % 200 == 0 && PhotonNetwork.IsConnected)
        {
            Debug.Log($"Current State: {PhotonNetwork.NetworkClientState}, Ping: {PhotonNetwork.GetPing()}ms");
        }
    }
}