using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SebPlayer : MonoBehaviour
{
    public Rigidbody2D m_Rbd;
    public float m_Speed;
    float m_Timer = 0f;
    bool m_ResetTimer;
    #region JumpStuff 
    public float m_FallingGravityRatio;  
    public float m_JumpingGravityRatio;   
    public float m_OnFloorGravityRatio; 
    public float m_JumpCancelingGravityRatio;  
    public float m_Jumpforce;
    [SerializeField]
    bool m_MustJump;
    [SerializeField]
    bool m_JumpButtonPressed;
    [SerializeField]
    bool m_OnGround;
    [SerializeField]
    bool m_IsJumping;
    [SerializeField]
    bool m_IsFalling;
    #endregion
    #region DashStuff
    public float m_DashForce;
    [SerializeField]
    bool m_MustDash;
    [SerializeField]
    bool m_MovingLeft;
    [SerializeField]
    bool m_MovingRight;
    #endregion

    void Start()
    {
        m_Rbd = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        ManageDash();
        ManageInput();
        ManageJump();
        ManageGravityRatio();
        m_IsFalling = !m_OnGround && m_Rbd.velocity.y <= 0;
    }
    public void ManageInput()
    {
        if (Input.GetKey(KeyCode.A) && !m_MustDash)
        {
            m_MovingLeft = true;
            m_Rbd.MovePosition(transform.position + transform.right * -m_Speed);
        }
        if (Input.GetKey(KeyCode.D) && !m_MustDash)
        {
            m_MovingRight = true;
            m_Rbd.MovePosition(transform.position + transform.right * m_Speed);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            m_MovingLeft = false;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            m_MovingRight = false;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_MustJump = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            m_MustDash = true;
        }
        m_JumpButtonPressed = Input.GetKey(KeyCode.Space);
    }
    #region DashManagement
    public void ManageDash()
    {
        if (m_MustDash)
        {
            Dash();
        }
    }
    public void Dash()
    {
        m_Timer += Time.deltaTime;

        if (m_Timer > 1.5f)
        {
            m_MustDash = false;
            m_ResetTimer = true;
        }
        if (m_ResetTimer)
        {
            m_Timer = 0f;
            m_ResetTimer = false;
        }
        if (m_MovingRight)
        {
            m_Rbd.AddForce(Vector3.right * m_DashForce, ForceMode2D.Impulse);
        }
        if (m_MovingLeft)
        {
            m_Rbd.AddForce(Vector3.right * -m_DashForce, ForceMode2D.Impulse);
        }
        m_Rbd.velocity = new Vector3(Mathf.Clamp(m_Rbd.velocity.x, -5f, 5f), m_Rbd.velocity.y);
    }
    #endregion
    #region JumpManagement
    public void ManageGravityRatio()
    {
        float desiredGravityRatio = m_OnFloorGravityRatio;
        if (m_IsJumping)
        {
            if (!m_JumpButtonPressed)
            {
                desiredGravityRatio = m_JumpCancelingGravityRatio;
            }
            else
            {
                desiredGravityRatio = m_JumpingGravityRatio;
            }
        }
        else if (m_IsFalling)
        {
            desiredGravityRatio = m_FallingGravityRatio;
        }
    }
    public void ManageJump()
    {
        if (m_MustJump)
        {
            m_MustJump = false;
            if (m_OnGround)
            {
                Jump();
            }
            else
            {
                if (m_Rbd.velocity.y <= 0)
                {
                    m_MustJump = false;
                }
            }
        }
    }
    public void Jump()
    {
        m_Rbd.AddForce(Vector3.up * m_Jumpforce, ForceMode2D.Impulse);
        m_IsJumping = true;
        m_IsFalling = false;
        m_OnGround = false;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        m_IsJumping = false;
        m_IsFalling = false;
        m_OnGround = true;
    }
    #endregion
}

