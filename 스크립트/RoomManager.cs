using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public GameObject Targets_Root;
    public GameObject Rooms_Root;

    public struct Obj
    {
        public GameObject obj;
        public bool IsUsed;
    };
    public Obj[] Rooms;
    public Obj[] Targets;

    // Start is called before the first frame update
    void Awake()
    {
        Targets = new Obj[Targets_Root.transform.childCount];
        Rooms = new Obj[Rooms_Root.transform.childCount];

        for (int i = 0; i < Targets.Length; i++)
        {
            Targets[i].obj = Targets_Root.transform.GetChild(i).gameObject;
            Targets[i].IsUsed = false;
        }

        for (int i = 0; i < Rooms.Length; i++)
        {
            int num = Random.Range(0, Targets.Length);
            while (Targets[num].IsUsed)
            {
                num = Random.Range(0, Targets.Length);
            }

            Rooms[i].obj = Rooms_Root.transform.GetChild(i).gameObject;
            Rooms[i].obj.transform.position = Targets[num].obj.transform.position;
            Targets[num].IsUsed = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {

            for (int i = 0; i < Targets.Length; i++)
            {
                Debug.Log(Targets[i].IsUsed);

            }
        }
    }

}
