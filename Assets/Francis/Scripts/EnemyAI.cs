using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Game Object Manager")]
    public EnemyManager m_Enemy;
    public Animator m_EnemyAnimator;
    public SpriteRenderer m_EnemySprite;
    public Transform m_EnemyTransform;
    public Rigidbody2D m_EnemyRigid;

    [Header("Enemy Ground Detect")]
    [SerializeField] private float m_GroundCheckSize;
    public Transform m_GroundCheckAiL;
    public Transform m_GroundCheckAiR;

    [Header("Enemy check player position")]
    [SerializeField] private float m_PlayerCheckDistance;
    [SerializeField] private LayerMask m_WhatIsPlayer;
    public Transform m_PlayerCheckRight;
    public Transform m_PlayerCheckLeft;

    [Header("Enemy Movement")]
    public float m_AiMoveSpeed;
    public float m_AiMoveDistance;


    [Header("Enemy Knockback Settings")]
    public float m_EnemyKnockback;
    public float m_KnockbackDuration;
    public float m_EnemyKnockbackKnockbackCount;
    [HideInInspector] public bool m_EnemyKBRight;

    private bool m_TargetFound;
    private bool m_EnemyAttacking;
    private bool m_EnemyIdle;
    [HideInInspector] public bool EnemyKnockback = false;

    public void Start()
    {
        m_EnemyIdle = false;
        m_EnemyAttacking = false;
        m_TargetFound = false;
        m_Enemy.m_MoveAi = false;
        m_Enemy.m_MovingLeft = true;
        StartCoroutine(IdleStart());

    }


    private void FixedUpdate()
    {
        if (m_Enemy.m_MoveAi)
        {
            if (!m_Enemy.m_MovingLeft)
            {
                MoveRight();
                RaycastHit2D groundInfoRight = Physics2D.Raycast(m_GroundCheckAiR.position, Vector2.down, m_GroundCheckSize);
                if (groundInfoRight.collider == null)
                {
                    m_Enemy.m_MovingLeft = true;
                }
            }
            if (m_Enemy.m_MovingLeft)
            {
                MoveLeft();
                RaycastHit2D groundInfoLeft = Physics2D.Raycast(m_GroundCheckAiL.position, Vector2.down, m_PlayerCheckDistance);
                if (groundInfoLeft.collider == null)
                {
                    m_Enemy.m_MovingLeft = false;
                }
            }
        }
        if (EnemyKnockback)
        {
            if (m_EnemyKBRight)
            {
                Debug.Log("Enemy knock back to RIGHT");
                m_EnemyRigid.velocity = new Vector2(m_EnemyKnockback, 0);
            }
            else
            {
                Debug.Log("Enemy knock back to LEFT");
                m_EnemyRigid.velocity = new Vector2(-m_EnemyKnockback, 0);
            }
        }
    }

    IEnumerator EnemyKnockbackStart()
    {
        StopCoroutine(EnemyKnockbackStart());
        m_Enemy.m_MoveAi = false;
        if (!m_EnemyKBRight)
        {
            m_EnemyAnimator.SetTrigger("EnemyHurtLeft");
        }
        else
        {
            m_EnemyAnimator.SetTrigger("EnemyHurtRight");
        }

        EnemyKnockback = true;
        m_EnemyKnockbackKnockbackCount = m_KnockbackDuration;
        m_EnemyKnockbackKnockbackCount -= Time.deltaTime;
        yield return new WaitForSeconds(0.3f);
        EnemyKnockback = false;
        m_Enemy.m_MoveAi = true;
        m_EnemyAnimator.SetTrigger("Idle");

    }
    public void CheckPlayerSide(bool isPlayerRight)
    {
        if(isPlayerRight)
        {
            m_EnemyKBRight = false;
        }   
        else
        {
            m_EnemyKBRight = true;
        }
        StartCoroutine(EnemyKnockbackStart());
    }

    public void MoveRight()
    {
        if(!m_EnemyIdle)
        {
            if (!m_EnemyAttacking && !EnemyKnockback)
            {
                m_EnemyAnimator.SetBool("WalkingLeft", false);
                m_EnemyAnimator.SetBool("WalkingRight", true);
                m_EnemyRigid.velocity = new Vector2(m_AiMoveSpeed * Time.deltaTime * 25.0f, m_EnemyRigid.velocity.y);
                if (!m_TargetFound)
                {
                    RaycastHit2D playerRight = Physics2D.Raycast(m_PlayerCheckRight.position, Vector2.right, m_PlayerCheckDistance, m_WhatIsPlayer);
                    if (playerRight)
                    {
                        m_EnemyAttacking = true;
                        m_TargetFound = true;
                        AttackPlayer();
                    }
                }
            }
        }
    }
    public void MoveLeft()
    {
        if(!m_EnemyIdle)
        {
            if (!m_EnemyAttacking && !EnemyKnockback)
            {
                m_EnemyAnimator.SetBool("WalkingRight", false);
                m_EnemyAnimator.SetBool("WalkingLeft", true);
                m_EnemyRigid.velocity = new Vector2(-m_AiMoveSpeed * Time.deltaTime * 25.0f, m_EnemyRigid.velocity.y);
                if (!m_TargetFound)
                {
                    RaycastHit2D playerLeft = Physics2D.Raycast(m_PlayerCheckLeft.position, Vector2.left, m_PlayerCheckDistance, m_WhatIsPlayer);
                    if (playerLeft)
                    {
                        m_TargetFound = true;
                        m_EnemyAttacking = true;
                        AttackPlayer();
                    }
                }
            }
        }
    }

    public void ManageMovement()
    {
        if (m_Enemy.m_MoveAi)
        {
            m_Enemy.m_MoveAi = false;
        }
        else
        {
            m_Enemy.m_MoveAi = true;
        }
    }

    IEnumerator IdleStart()
    {
        m_EnemyAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f);
        ManageMovement();
    }

    public void AttackPlayer()
    {
        m_EnemyAnimator.SetBool("IsAttacking", true);
        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        m_EnemyRigid.velocity = Vector2.zero;
        m_EnemyAnimator.SetTrigger("Attack");
        m_EnemyIdle = true;
        m_TargetFound = false;
        m_EnemyAttacking = false;
        yield return new WaitForSeconds(.2f);   
        StartCoroutine(IdleForTime());
        Collider2D player = Physics2D.OverlapCircle(m_PlayerCheckLeft.position, 1f, m_WhatIsPlayer);
        if(player)
        {
            var playerScript = player.GetComponent<PlayerController>();
            playerScript.m_PlayerKnockbackCount = playerScript.m_KnockbackDuration;
            playerScript.m_GotHit = true;
            playerScript.GetDamage(m_Enemy.m_EnemyDamage);
            if (player.transform.position.x < transform.position.x)
            {
                playerScript.m_PlayerKBRight = true;
            }
            else
            {
                playerScript.m_PlayerKBRight = false;
            }
        }  
    }
    IEnumerator IdleForTime()
    {
        m_EnemyAnimator.SetBool("WalkingRight", false);
        m_EnemyAnimator.SetBool("WalkingLeft", false);
        yield return new WaitForSeconds(2.0f);
        m_EnemyAnimator.SetBool("IsAttacking", false);
        m_EnemyAnimator.SetTrigger("Idle");
        m_EnemyIdle = false;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(m_PlayerCheckLeft.position, .01f);
    }
}
