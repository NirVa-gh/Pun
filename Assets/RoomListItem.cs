using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    private RoomInfo _roomInfo;

    public void SetUp(RoomInfo roomInfo)
    {
        _roomInfo = roomInfo;
        _text.text = roomInfo.Name;
    }
    
    public void OnClick()
    {
        NetworkManager.Instance.JoinRoom(_roomInfo);
    }


}
