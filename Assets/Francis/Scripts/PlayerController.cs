using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{

    [Header("References to Transform")]
    public Transform m_LeftWallDectect;
    public Transform m_RightWallDectect;
    public Transform m_ExecutionCheck;
    private Transform m_CurrentyY;

    [Header("Reference to other")]
    public Animator m_PlayerAnimator;
    public SpriteRenderer m_PlayerSpriteRend;
    public HitBoxManager m_HitBoxManager;
    public GameObject m_SceneManage;
    [HideInInspector] public Collider2D m_EnemyHit;
    private Rigidbody2D m_PlayerRigid2D;


    [Header("Particle System")]
    public ParticleSystem m_DashEffectRight;
    public ParticleSystem m_DashEffectLeft;
    public ParticleSystem m_FootStepsEffect;
    public ParticleSystem m_LandingEffect;
    private ParticleSystem.EmissionModule m_FootStepsEmission;

    [Header("Layer Mask")]
    public LayerMask m_WhatIsGround;
    public LayerMask m_EnemyLayers;
    public LayerMask m_GroundLayers;

    [Header("Player Stats")]
    public float m_PlayerMaxHP;
    public float m_PlayerCurrentHP;
    public int m_PlayerLives;
    public Slider m_HealthBar;

    [Header("Attack settings")]
    public Transform m_AttackHitBox;
    public float m_AttackSize;
    public float m_PlayerAttackSpeed;
    public float m_PlayerNextAttack;

    [Tooltip("The size that the player can execute enemies")]
    public float m_ExecutionSize;

    [Header("Knockback Settings")]
    public float m_PlayerKnockback;
    public float m_KnockbackDuration;
    public float m_PlayerKnockbackCount;
    public float m_PlayerKnockBackDownSpeed;

    [Header("Collisions")]
    public float m_GroundCheckLength;
    public Vector3 m_ColliderOffset;

    [Header("Jump Settings")]
    [SerializeField] private float m_HangTime;
    [SerializeField] private float m_JumpBufferLength;
    [SerializeField] private float m_PlayerMinJump;
    [SerializeField] private float m_PlayerMaxJump;
    public float m_PlayerJumpSpeed;
    public float m_PlayerFallSpeed;
    public float m_Gravity;

    [Header("Invicibility Timer")]
    [SerializeField] private float m_InvicibilityTime;

    [Header("Player movement Settings")]
    [SerializeField] private float m_PlayerMoveSpeed;

    [Header("Player Dash Settings")]
    [SerializeField] private float m_DashSpeed = 10.0f;
    [SerializeField] private float m_DashDuration = 0.1f;
    public float m_DashCooldown = 2.0f;
    public Text m_DashCooldowndisplay;
    public Color m_DashColorFeedback;

    [HideInInspector] public bool m_IsJumping;   
    [HideInInspector] public bool m_PlayerKBRight;
    [HideInInspector] public bool m_IsSpriteLookingRight;
    [HideInInspector] public bool m_GotHit = false;
    [HideInInspector] public bool m_IsDashing;
    [HideInInspector] public bool m_PlayerTeleporting = false;
    private float m_HorizontalSpeed;
    private float m_HangCounter;
    private float m_JumpBufferCounter;
    [SerializeField] private bool m_IsGrounded;
    private bool m_CanFlipSprite = true;
    private bool m_WasOnGround;
    private bool m_CanDash;
    private bool m_RemoveControl = false;
    private bool m_KBAddForceDown;
    private bool m_ModifyPhys = false;
    private float m_TurnLeft = 0.0f;
    private float m_TurnRight = 0.0f;
    private bool m_MoveRecently = false;
    private Coroutine m_DashCoroutine;


    public void Start()
    {
        m_HealthBar.value = m_PlayerMaxHP;
        m_PlayerCurrentHP = m_HealthBar.value;
        m_PlayerRigid2D = GetComponent<Rigidbody2D>();
        m_CanDash = true;
        m_FootStepsEmission = m_FootStepsEffect.emission;
        m_IsSpriteLookingRight = false;
    }

    public void Update()
    {
        //Check if the player is on ground
        m_IsGrounded = Physics2D.Raycast(transform.position + m_ColliderOffset, Vector2.down, m_GroundCheckLength, m_WhatIsGround) || Physics2D.Raycast(transform.position - m_ColliderOffset, Vector2.down, m_GroundCheckLength, m_WhatIsGround);

        //Sets a timer when not on the ground. Before that timer expires you can jump even if not on ground.
        if (m_IsGrounded)
        {
            m_PlayerRigid2D.gravityScale = m_Gravity;
            m_PlayerAnimator.SetBool("IsAirBorn", false);
            m_IsJumping = false;
            m_HangCounter = m_HangTime;
            m_ModifyPhys = false;
        }
        else
        {
            m_ModifyPhys = true;
            m_PlayerAnimator.SetBool("IsAirBorn", true);
            m_HangCounter -= Time.deltaTime;
        }

        //Call the Dash Function
        if (Input.GetKeyDown(KeyCode.LeftShift) && m_CanDash)
        {
            if (m_DashCoroutine != null)
            {
                StopCoroutine(m_DashCoroutine);
                m_DashCoroutine = null;
            }
            m_DashCoroutine = StartCoroutine(Dash());       
        }

        //Call Attack function
        if(Time.time >= m_PlayerNextAttack)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                StartCoroutine(Attack());
                m_PlayerNextAttack = Time.time + 1f / m_PlayerAttackSpeed;
            }
        }
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            CheckForExecution();
        }



        //Checks if player pressed jump before landing on the floor and the timer will start the jump function
        if (Input.GetKeyDown(KeyCode.Space) && !m_RemoveControl)
        {       
            m_JumpBufferCounter = m_JumpBufferLength;
            m_IsJumping = true;
        }
        else
        {
            m_JumpBufferCounter -= Time.deltaTime;
        }
        //If the player release the jump early it will make them jump less high than hold
        if (Input.GetKeyUp(KeyCode.Space) && m_PlayerRigid2D.velocity.y > 0f && !m_RemoveControl)
        {
            m_PlayerRigid2D.velocity = new Vector2(m_PlayerRigid2D.velocity.x, m_PlayerRigid2D.velocity.y * m_PlayerMinJump);  
        }

        //Manages when the player wants to jump and also checks if the player wants to buffer a jump
        if (m_JumpBufferCounter >= 0f && m_HangCounter > 0f && !m_RemoveControl)
        {
            m_PlayerRigid2D.velocity = new Vector2(m_PlayerRigid2D.velocity.x, 0f);
            m_PlayerRigid2D.AddForce(Vector2.up * m_PlayerJumpSpeed, ForceMode2D.Impulse);
            m_JumpBufferCounter = 0f;
        }

        //Set Falling or jump animation
        if (m_PlayerRigid2D.velocity.y < -0.01 && m_IsJumping && !m_RemoveControl)
        {
            m_PlayerAnimator.SetBool("IsAirBorn", false);
            m_PlayerAnimator.SetTrigger("Falling");
        }
        else if (m_PlayerRigid2D.velocity.y > -0.01 && m_IsJumping && !m_RemoveControl)
        {
            m_PlayerAnimator.SetTrigger("Jump");
        }

        if (m_ModifyPhys)
        {
            ModifyPhysics();
        }

        //Flip the sprite of the player when moving in different directions and switch the Attack Hitbox around
        if(Input.GetAxisRaw("Horizontal") > 0 && !m_RemoveControl)
        {
            if(m_CanFlipSprite)
            {
                m_CanFlipSprite = false;
                WallDetectRight();
                StartCoroutine(FlipSprite());
                m_IsSpriteLookingRight = true;
                m_HitBoxManager.ManageHitBox(true);
            }         
        }
        else if(Input.GetAxisRaw("Horizontal") < 0 && !m_RemoveControl)
        {
            if (m_CanFlipSprite)
            {
                m_CanFlipSprite = false;
                WallDetectLeft();
                StartCoroutine(FlipSprite());
                m_IsSpriteLookingRight = false;
                m_HitBoxManager.ManageHitBox(false);
            }        
        }



        //Show footstep effect
        if(Input.GetAxisRaw("Horizontal") != 0 && m_IsGrounded && !m_RemoveControl)
        {
            m_FootStepsEmission.rateOverTime = 35f;
        }
          else
        {
            m_FootStepsEmission.rateOverTime = 0;
        }

        //Show the impact on ground effect
        if(!m_WasOnGround && m_IsGrounded && !m_RemoveControl)
        {
            m_LandingEffect.gameObject.SetActive(true);
            m_LandingEffect.Stop();
            m_LandingEffect.transform.position = m_FootStepsEffect.transform.position;
            m_LandingEffect.Play();
        }
        m_WasOnGround = m_IsGrounded;


        //Animation Part

        //Set a float that checks if the player is running
        m_HorizontalSpeed = Input.GetAxisRaw("Horizontal") * m_PlayerMoveSpeed;
        if(m_PlayerRigid2D.velocity.x != 0)
        {
            m_PlayerAnimator.SetFloat("Speed", m_HorizontalSpeed);
        }
        else
        {
            m_PlayerAnimator.SetFloat("Speed", 0);
        }

        //Checks if player is grounded or jumping and plays the correct animation
        if(!m_IsGrounded)
        {
            m_PlayerAnimator.SetBool("IsGrounded", false);
        }
        else
        {
            m_PlayerAnimator.SetBool("IsGrounded", true);
        }    
        if(m_IsJumping)
        {
            m_PlayerAnimator.SetTrigger("Jump");
        }
        if (m_PlayerTeleporting)
        {
            m_IsDashing = false;
            m_PlayerRigid2D.velocity = Vector2.zero;
            m_PlayerTeleporting = false;
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            m_SceneManage.GetComponent<SceneLoader>().RetryProto();
        }

        if(!m_IsGrounded && m_PlayerRigid2D.velocity.y == 0.0f && !m_IsDashing)
        {
            float newGround = m_GroundCheckLength;
            m_GroundCheckLength = 0.6f;
            m_GroundCheckLength = newGround;
        }
    }

    public void FixedUpdate()
    {
        //Move player from left to right, stock the horizontal speed and start run animation
        if (!m_RemoveControl && m_PlayerKnockbackCount <= 0)
        {
            if(m_IsGrounded)
            {               
                m_PlayerRigid2D.velocity = new Vector2(m_HorizontalSpeed, m_PlayerRigid2D.velocity.y);
            }
            else if(!m_IsGrounded)
            {
                m_PlayerRigid2D.velocity = new Vector2(m_HorizontalSpeed, m_PlayerRigid2D.velocity.y);
            }    
            
        }
        else
        {
            m_PlayerRigid2D.isKinematic = true;
            m_PlayerRigid2D.isKinematic = false;
        }


        //Manage KnockBack
        if (m_GotHit)
        {
            StartCoroutine(MakeInvicible());
            StartCoroutine(KnockBack());
            if (m_PlayerKBRight)
            {
                m_PlayerRigid2D.velocity = new Vector2(-m_PlayerKnockback, m_PlayerKnockback);
            }
            else
            {
                m_PlayerRigid2D.velocity = new Vector2(m_PlayerKnockback, m_PlayerKnockback);
            }
        }
        if (m_KBAddForceDown)
        {
            m_PlayerRigid2D.velocity = new Vector2(m_PlayerRigid2D.velocity.x, -m_PlayerKnockBackDownSpeed);
        }
    }

    IEnumerator FlipSprite()
    {
        yield return new WaitForSeconds(0.1f);
        if(m_HorizontalSpeed > 0.0f)
        {
            m_PlayerSpriteRend.flipX = false;
        }
        else if(m_HorizontalSpeed < 0.0f)
        {
            m_PlayerSpriteRend.flipX = true;
        }
        m_CanFlipSprite = true;
    }

    public void ModifyPhysics()
    {
        if (m_PlayerRigid2D.velocity.y < 0f)
        {
            m_PlayerRigid2D.gravityScale = m_Gravity * m_PlayerFallSpeed;
        }
        else if(m_PlayerRigid2D.velocity.y > 0f && !Input.GetButton("Jump"))
        {
            m_PlayerRigid2D.gravityScale = m_Gravity * (m_PlayerFallSpeed / 2f);
        }
        else
        {
            m_PlayerRigid2D.gravityScale = m_Gravity;
        }
    }

    //Dash Code
    IEnumerator Dash()
    {
        
        m_PlayerSpriteRend.material.color = m_DashColorFeedback;
        m_RemoveControl = true;
        m_IsDashing = true;
        m_CanDash = false;

        float timer = 0f;

        while (timer < m_DashDuration) 
        {
            if (!m_PlayerSpriteRend.flipX)
            {
                //m_DashEffectRight.gameObject.SetActive(true);
                m_PlayerRigid2D.velocity = new Vector2(m_DashSpeed * Time.deltaTime * 100.0f, 0f);
            }
            else
            {
                //m_DashEffectLeft.gameObject.SetActive(true);
                m_PlayerRigid2D.velocity = new Vector2(-m_DashSpeed * Time.deltaTime * 100.0f, 0f);
            }
            
            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        m_IsDashing = false;
        m_RemoveControl = false;
        //m_DashEffectLeft.gameObject.SetActive(false);
        //m_DashEffectRight.gameObject.SetActive(false);
        yield return new WaitForSeconds(m_DashCooldown);
        m_PlayerSpriteRend.material.color = Color.white;
        //m_DashEffect.gameObject.SetActive(false);
        ///m_PlayerAnimator.SetTrigger("CanDash"); Failed test
        m_CanDash = true;
        // m_PlayerSpriteRend.color = Color.black;
    }

    //SetPlayer Invincible
    IEnumerator MakeInvicible()
    {
        StartCoroutine(FlashSprite());
        gameObject.layer = 12;
        yield return new WaitForSeconds(m_InvicibilityTime);
        gameObject.layer = 8;
    }

    

    IEnumerator FlashSprite()
    {
        m_PlayerSpriteRend.enabled = false;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = true;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = false;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = true;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = false;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = true;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = false;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = true;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = false;
        yield return new WaitForSeconds(0.2f);
        m_PlayerSpriteRend.enabled = true;
        yield return new WaitForSeconds(0.2f);
    }

    //Does the Knockback
    IEnumerator KnockBack()
    {
        m_RemoveControl = true;
        m_PlayerKnockbackCount -= Time.deltaTime;
        yield return new WaitForSeconds(0.2f);
        m_KBAddForceDown = true;
        yield return new WaitForSeconds(0.2f);
        m_KBAddForceDown = false;
        m_GotHit = false;
        m_RemoveControl = false;
    }

    //Handles the attack for the player
    IEnumerator Attack()
    {
        //If player is on ground attack
        if(m_IsGrounded)
        {
            //Freeze sprite flip
            m_CanFlipSprite = false;
            //Start Animation
            m_PlayerAnimator.SetTrigger("Attack");
            m_PlayerAnimator.SetBool("IsAttacking", true);
            float timer = 0f;

            while (timer < 0.2f)
            {
                m_RemoveControl = true;
                if (m_IsGrounded)
                {
                    m_PlayerRigid2D.velocity = new Vector2(0f, 0f);
                }
                timer += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            //Detects enemy in the Circle HitBox and makes an array
            Collider2D[] m_EnemyHit = Physics2D.OverlapCircleAll(m_HitBoxManager.m_PlayerAttackHitBox.position, m_AttackSize, m_EnemyLayers);

            //Calls the Enemy Manager and gives fear to all that collided with player attack
            foreach (Collider2D enemy in m_EnemyHit)
            {
                if (enemy.isTrigger == true)
                {
                    enemy.GetComponent<EnemyManager>().ReceiveFear(enemy.GetComponent<EnemyManager>().m_PlayerFearDamage);
                    enemy.GetComponent<EnemyAI>().m_Enemy.m_MoveAi = false;
                    if (transform.position.x < enemy.transform.position.x)
                    {
                        enemy.GetComponent<EnemyAI>().CheckPlayerSide(false);
                    }
                    else
                    {
                        enemy.GetComponent<EnemyAI>().CheckPlayerSide(true);
                    }
                }
            }
            yield return new WaitForSeconds(.1f);
            m_PlayerAnimator.SetBool("IsAttacking", false);
            m_RemoveControl = false;
            m_CanFlipSprite = true;
        }
        //If player is in the air attack
        else
        {
            //Freeze sprite flip
            m_CanFlipSprite = false;
            //Start Animation
            m_PlayerAnimator.SetTrigger("Attack");
            m_PlayerAnimator.SetBool("IsAttacking", true);
            yield return new WaitForSeconds(0.3f);
            m_RemoveControl = false;
            //Detects enemy in the Circle HitBox and makes an array
            Collider2D[] m_EnemyHit = Physics2D.OverlapCircleAll(m_HitBoxManager.m_PlayerAttackHitBox.position, m_AttackSize, m_EnemyLayers);

            //Calls the Enemy Manager and gives fear to all that collided with player attack
            foreach (Collider2D enemy in m_EnemyHit)
            {
                if (enemy.isTrigger == true)
                {
                    enemy.GetComponent<EnemyManager>().ReceiveFear(enemy.GetComponent<EnemyManager>().m_PlayerFearDamage);
                    enemy.GetComponent<EnemyAI>().m_Enemy.m_MoveAi = false;
                    if (transform.position.x < enemy.transform.position.x)
                    {
                        enemy.GetComponent<EnemyAI>().CheckPlayerSide(false);
                    }
                    else
                    {
                        enemy.GetComponent<EnemyAI>().CheckPlayerSide(true);
                    }
                }
            }
            yield return new WaitForSeconds(.1f);
            m_PlayerAnimator.SetBool("IsAttacking", false);
            m_CanFlipSprite = true;
        }
    }



    //Finds closest target, calls to check if target can be executed
    public void CheckForExecution()
    {
        float distanceToClosestEnemy = Mathf.Infinity;
        Collider2D m_ClosestEnemy = null;
        Vector3 currentPosition = transform.position;

        //make an array of all nearby enemy and start checking if can be executed
        Collider2D[] executeEnemy = Physics2D.OverlapCircleAll(m_ExecutionCheck.position, m_ExecutionSize, m_EnemyLayers);

        //Checks which target is closest to the player
        foreach (Collider2D currentEnemy in executeEnemy)
        {
            float distanceToTarget = Vector3.Distance(currentEnemy.transform.position, currentPosition);
            if(distanceToTarget < distanceToClosestEnemy)
            {
                if(currentEnemy.GetComponent<EnemyManager>().m_CanBeExecuted)
                {
                  distanceToClosestEnemy = distanceToTarget;
                  m_ClosestEnemy = currentEnemy;           
                }
            }          
        }
        if(m_ClosestEnemy != null)
        {
            Debug.Log(m_ClosestEnemy);
            m_ClosestEnemy.GetComponent<EnemyManager>().CheckForExecute();
        } 

    }

    //Check if player is running in a right wall and if so set de velocity to 0
    public void WallDetectRight()
    {
       bool rightWall = Physics2D.OverlapCircle(m_RightWallDectect.position, .01f, m_GroundLayers);
        if(rightWall)
        {
            Vector2 velo = m_PlayerRigid2D.velocity;
            velo.x = 0f;
            m_PlayerRigid2D.velocity = velo;
        }
    }

    //Check if player is running in a left wall and if so set de velocity to 0
    public void WallDetectLeft()
    {
      bool leftWall = Physics2D.OverlapCircle(m_LeftWallDectect.position, .01f, m_GroundLayers);
        if(leftWall)
        {
          Vector2 velo = m_PlayerRigid2D.velocity;
            velo.x = 0f;
            m_PlayerRigid2D.velocity = velo;
        }
    }

    //Player get damage
    public void GetDamage(int damage)
    {
        m_PlayerCurrentHP -= damage;
        m_HealthBar.value = m_PlayerCurrentHP;
        Debug.Log(m_PlayerCurrentHP);
        if(m_PlayerCurrentHP <= 0)
        {
            StartCoroutine(PlayerDeath());
            m_PlayerCurrentHP = 0;
            m_PlayerAnimator.SetTrigger("Death");
        }
    }
    IEnumerator PlayerDeath()
    {
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(1.5f);
        m_SceneManage.GetComponent<SceneLoader>().PlayedDied();
    }

    //Draws gizmo in editor
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(m_HitBoxManager.m_PlayerAttackHitBox.position, m_AttackSize);
        Gizmos.DrawWireSphere(m_ExecutionCheck.position, m_ExecutionSize);
        Gizmos.DrawWireSphere(m_LeftWallDectect.position, .01f);
        Gizmos.DrawWireSphere(m_RightWallDectect.position, .01f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + m_ColliderOffset, transform.position + m_ColliderOffset + Vector3.down * m_GroundCheckLength);
        Gizmos.DrawLine(transform.position - m_ColliderOffset, transform.position - m_ColliderOffset + Vector3.down * m_GroundCheckLength);
        if (m_AttackHitBox == null)
        {
            return;
        }
        if (m_ExecutionCheck == null)
        {
            return;
        }
    }
}
