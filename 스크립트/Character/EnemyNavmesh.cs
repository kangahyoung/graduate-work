using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavmesh : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        NavMeshAgent nav = GetComponent<NavMeshAgent>();
        
    }

    // Update is called once per frame
    void Update()
    {
        NavMesh.AddNavMeshData();
    }
}
