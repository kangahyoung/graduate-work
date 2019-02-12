using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : CharacterBase
{
//    public PlayerBase instance;
    [SerializeField] protected Camera cam;


    protected Rigidbody m_RigidBody;
    protected CapsuleCollider m_Collider;

    [Header("Move")]
    [SerializeField] protected float PlayerCurrentSpeed = 0f;
    public float ForwardSpeed = 4.0f;   // Speed when walking forward
    public float BackwardSpeed = 2.0f;  // Speed when walking backwards
    public float StrafeSpeed = 2.0f;    // Speed when walking sideways
    public float RunMultiplier = 2.0f;   // Speed when sprinting

    public KeyCode RunKey = KeyCode.LeftShift;
    public KeyCode CrouchKey = KeyCode.LeftControl;

    public bool m_Running;
    public bool m_Crouching;

    public float xSensitivity = 1.0f;
    public float ySensitivity = 1.0f;

    public float rotSpeed = 3.0f;

    // Start is called before the first frame update
    void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Inspector

    private void Move()
    {

    }

    //private Vector2 GetInput()
    //{

    //    Vector2 input = new Vector2
    //    {

    //        x = CrossPlatformInputManager.GetAxis("Horizontal"),
    //        y = CrossPlatformInputManager.GetAxis("Vertical")
    //    };
    //    movementSettings.UpdateDesiredTargetSpeed(input);
    //    return input;
    //}

    #endregion
}
