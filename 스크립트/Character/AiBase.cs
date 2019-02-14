using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiBase : ObjectBase
{
    NavMeshAgent m_Agent;

    public float RangeSearching;
    public GameObject Targets_Root;
    public GameObject[] Target;

    public enum State : int{ Idle, Chasing, Moving };

    public State AiState;

    public int TargetPos;
    // Start is called before the first frame update
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();

        AiState = State.Idle;

        Target = new GameObject[Targets_Root.transform.childCount];
        for (int i = 0; i < Targets_Root.transform.childCount; i++) {
            Target[i] = Targets_Root.transform.GetChild(i).gameObject;
        }

        FindTarget();
    }

    // Update is called once per frame
    void Update()
    {

        if (AiState == State.Idle)
        {
            FindTarget();
        }
        //if (Target[TargetPos].transform.position == transform.position)
        //{
        //    AiState = State.Idle;
        //}

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawSphere(transform.position, RangeSearching);
    }

    public void FindTarget()
    {
        AiState = State.Moving;
        TargetPos = Random.Range(0, Target.Length);
        m_Agent.destination = Target[TargetPos].transform.position;


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Target")
        {
            if(other.gameObject == Target[TargetPos])
            {
                Debug.Log(other.name);
                AiState = State.Idle;
            }
        }
    }
}
