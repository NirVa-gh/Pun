using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;

public class TestCreateRoom : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");

        // Добавьте задержку перед созданием комнаты
        StartCoroutine(CreateRoomWithDelay());
    }

    IEnumerator CreateRoomWithDelay()
    {
        yield return new WaitForSeconds(1f); // Задержка 1 секунда

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        // Добавьте дополнительные настройки
        roomOptions.PlayerTtl = 30000;
        roomOptions.EmptyRoomTtl = 10000;

        PhotonNetwork.CreateRoom("TestRoom_" + System.Guid.NewGuid().ToString().Substring(0, 8), roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined room: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create Room Failed: {returnCode} - {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected: {cause}");
    }
}