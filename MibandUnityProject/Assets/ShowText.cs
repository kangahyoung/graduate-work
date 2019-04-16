using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour
{
  public Text BPMText;
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
      BPMText.text = "Now BPM : " + NetworkManager.Instance.NowBPM.ToString();
  }
}
