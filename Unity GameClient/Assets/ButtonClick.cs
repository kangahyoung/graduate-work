using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static publicPacketProtocol.PacketProtocol;

public class ButtonClick : MonoBehaviour {

  public Dropdown dropdown;
  public Text dropdownText;
  public void buttonClick() {
        //dropdown.te
        Debug.Log(":dd");
    if (dropdownText.text == "연결하기") {
      Debug.Log(dropdown.options[dropdown.value].text + "( " + dropdown.value + ")");
      publicPostData clientPostData = new publicPostData();
      clientPostData.packet_Type = (int)ClientPacketType.ConnectBLE;
      clientPostData.data = dropdown.value;
      NetworkManager.Send(NetworkManager.GetSocket(), Packet.Serialize(clientPostData));
      dropdownText.text = "일시정지";
    } else if (dropdownText.text == "일시정지") {
      publicPostData clientPostData = new publicPostData();
      clientPostData.packet_Type = (int)ClientPacketType.StopBLE;
      clientPostData.data = 0;
      NetworkManager.Send(NetworkManager.GetSocket(), Packet.Serialize(clientPostData));
      dropdownText.text = "다시시작";
    } else if (dropdownText.text == "다시시작") {
      publicPostData clientPostData = new publicPostData();
      clientPostData.packet_Type = (int)ClientPacketType.StartBLE;
      clientPostData.data = 0;
      NetworkManager.Send(NetworkManager.GetSocket(), Packet.Serialize(clientPostData));
      dropdownText.text = "일시정지";
    }


  }

  // Start is called before the first frame update
  void Start() {

  }

}
