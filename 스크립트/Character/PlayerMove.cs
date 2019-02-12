﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : PlayerBase
{
    Animator animator;
    public Vector2 input;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();

        Rotate();
    }

    private void Update()
    {
        animator.SetBool("Runnig", m_Running);
        animator.SetBool("Crouching", m_Crouching);
        animator.SetFloat("Ypos", input.y);
    }
    #region Function

    private void Move()
    {
        Vector2 input = GetInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon))
        {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * input.y + transform.right * input.x;
            //            desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

            desiredMove.x = desiredMove.x * PlayerCurrentSpeed * Time.deltaTime;
            desiredMove.z = desiredMove.z * PlayerCurrentSpeed * Time.deltaTime;
            desiredMove.y = desiredMove.y * PlayerCurrentSpeed * Time.deltaTime;
            if (m_RigidBody.velocity.sqrMagnitude < (PlayerCurrentSpeed * PlayerCurrentSpeed))
            {
//                m_RigidBody.AddForce(desiredMove, ForceMode.Impulse);
                m_RigidBody.MovePosition(transform.position + desiredMove);
            }
        }
    }
    
    private void Rotate()
    {
        float rotX = Input.GetAxis("Mouse Y") * rotSpeed;
        float rotY = Input.GetAxis("Mouse X") * rotSpeed;

        this.transform.localRotation *= Quaternion.Euler(0, rotY, 0);
        cam.transform.localRotation *= Quaternion.Euler(-rotX,0, 0);
    }

    private Vector2 GetInput()
    {

        input = new Vector2
        {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };

        UpdateDesiredTargetSpeed(input);
        return input;
    }

    public void UpdateDesiredTargetSpeed(Vector2 input)
    {
   //     if (input == Vector2.zero) return;
        if (input.x > 0 || input.x < 0)
        {
            //strafe
            PlayerCurrentSpeed = StrafeSpeed;
        }
        if (input.y < 0)
        {
            //backwards
            PlayerCurrentSpeed = BackwardSpeed;
        }
        if (input.y > 0)
        {
            //forwards
            //handled last as if strafing and moving forward at the same time forwards speed should take precedence

            PlayerCurrentSpeed = ForwardSpeed;
        }
        if (Input.GetKey(RunKey))
        {
            PlayerCurrentSpeed *= RunMultiplier;
            m_Running = true;
        }
        else
        {
            m_Running = false;
        }

        if (Input.GetKey(CrouchKey))
        {
            if (!m_Crouching)
            {

                // 앉았을 때 콜라이더 높이 변경
                Vector3 center = m_Collider.center;
                center = new Vector3(m_Collider.center.x, m_Collider.center.y / 2, m_Collider.center.z);
                m_Collider.center = center;
                m_Collider.height /= 2;
                Debug.Log(center);

            }

            PlayerCurrentSpeed /= RunMultiplier;
            m_Crouching = true;
        }
        else
        {
            if (m_Crouching)
            {
                // 일어났을 때 콜라이더 높이 변경
                Vector3 center = m_Collider.center;
                center = new Vector3(m_Collider.center.x, m_Collider.center.y * 2, m_Collider.center.z);
                Debug.Log(center);
                m_Collider.center = center;
                m_Collider.height *= 2;
            }

            m_Crouching = false;

        }
    }

    
    
    #endregion
}
