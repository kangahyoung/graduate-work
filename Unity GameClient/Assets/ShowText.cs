using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour {
  public Text BPMText;
  public Dropdown dropdown;
  // Start is called before the first frame update
  void Start() {
    dropdown.ClearOptions();
  }

  // Update is called once per frame
  void Update() {
    BPMText.text = "Now BPM : " + NetworkManager.Instance.NowBPM.ToString();
    if (dropdown.options.Count == 0) {
      dropdown.AddOptions(NetworkManager.Instance.BleList);
    }
  }
}
