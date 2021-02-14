using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpManager : MonoBehaviour
{


    [Header("Character Ground Settings")]
    public Transform m_GroundCheck;
    public LayerMask m_WhatIsGround;
    public float m_GroundRadius;

    [Header("Character Jump Settings")]
    public float m_JumpForce;
    public float m_JumpSpeed;
    public float m_JumpTime;
    public float m_JumpTimeCount;

    [SerializeField] private bool m_OnGround;
    private Rigidbody2D m_CharRb;
    private float m_MoveInput;
    [SerializeField] private bool m_CharJumping;

    private void Start()
    {
        m_CharRb = GetComponent<Rigidbody2D>();
    }


    public void Update()
    {
        m_OnGround = Physics2D.OverlapCircle(m_GroundCheck.position, m_GroundRadius);
        if(m_OnGround && Input.GetKeyDown(KeyCode.Space))
        {
            m_CharJumping = true;
            m_OnGround = false;
            m_JumpTimeCount = m_JumpTime;
            m_CharRb.velocity = new Vector2(m_CharRb.velocity.x, m_JumpForce);
        }
        if(m_CharJumping && Input.GetKey(KeyCode.Space))
        {
            if(m_JumpTimeCount > 0)
            {
                m_CharRb.velocity = new Vector2(m_CharRb.velocity.x, m_JumpForce);
                m_JumpTimeCount -= Time.deltaTime;
            }
            else if(m_JumpTimeCount < 0)
            {
                m_CharJumping = false;
            }    
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {
            m_CharJumping = false;
        }
    }

    public void FixedUpdate()
    {
        



    }

}
