using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetForAim : MonoBehaviour
{
    public int TargetLayer;
    public Image interactionUI;
    public GameObject TargetObject;

    Dictionary<KeyCode, Action> keyDictionary;

    // Start is called before the first frame update
    void Start()
    {
        if (interactionUI != null)
        {
            interactionUI.enabled = false;
        }

        keyDictionary = new Dictionary<KeyCode, Action>
        {
            { KeyCode.Mouse0, KeyDown_E },
            { KeyCode.E, KeyDown_E }
        };
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach(var dic in keyDictionary)
            {
                if (Input.GetKeyDown(dic.Key))
                {
                    dic.Value();
                }

            }
        }
    }

    void FixedUpdate()
    {
//        Debug.DrawRay(transform.position, transform.forward * 5f, Color.red);
        PointingTarget();
    }
    

    public void KeyDown_E()
    {
        if(TargetObject != null && interactionUI.enabled == true)
        {

            bool State = TargetObject.GetComponent<Animator>().GetBool("Open");

            TargetObject.GetComponent<Animator>().SetBool("Open", !(State));
        }
    }

    public void PointingTarget()
    {
        RaycastHit hit;
        int mask = 1 << TargetLayer;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 7f, mask) && hit.collider.isTrigger)
        {
            Debug.DrawRay(transform.position, transform.forward * 7f, Color.red);

            TargetObject = hit.transform.gameObject;

            if (interactionUI != null)
            {
                interactionUI.enabled = true;
            }

            // 아이템일 경우 쉐이더 변경
            if(TargetObject.tag == "Item")
            {
                MeshRenderer mr = TargetObject.GetComponent<MeshRenderer>();   // 일단 MeshRenderer 컴포넌트를 얻고

                mr.material.shader = Shader.Find("Custom/OutLine");                                 // 쉐이더를 찾아(이름으로) 변경
            }
        }
        else
        {
            if (interactionUI != null)
            {
                interactionUI.enabled = false;
            }

            if (TargetObject.tag == "Item")
            {
                MeshRenderer mr = TargetObject.GetComponent<MeshRenderer>();   // 일단 MeshRenderer 컴포넌트를 얻고

                mr.material.shader = Shader.Find("Standard");                                 // 쉐이더를 찾아(이름으로) 변경
            }


            TargetObject = null;
        }
        
    }
}
