using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : ExecutionManager
{

    [Header("Game Objects")]
    public Transform m_PlayerTeleport;
    public GameObject m_VirtualCameraEnemy;
    public ParticleSystem m_ICanBeExecuted;
    [HideInInspector] public EnemyAI m_EnemyAI;

    [Header("Enemy Stats")]
    public int m_FearHp;
    public Slider m_FearBar;
    [Tooltip("Player's Fear Damage on the Enemy")]
    public int m_PlayerFearDamage;
    public int m_EnemyDamage;
    [Tooltip("How much fear does the enemy need to be executed")]
    public int m_FearExecutionThreshold;
    [Tooltip("How much damage does the propagation cause")]
    public int m_FearPropagationDamage;

    [HideInInspector] public bool m_CanBeExecuted;
    [HideInInspector] public bool m_MoveAi = true;
    [HideInInspector] public bool m_MovingLeft;
    [HideInInspector] public bool m_MovingRight;


    public void Start()
    {
        m_FearBar.value = m_FearHp;
        m_CanBeExecuted = false;
    }

    //Give fear damage and manage it
    public void ReceiveFear(int fearDamage)
    {
            m_FearBar.gameObject.SetActive(true);
            m_FearHp -= fearDamage;
            m_FearBar.value = m_FearHp;
            if (m_FearHp <= m_FearExecutionThreshold)
            {
                SetFearEffect();
                Debug.Log("Can be executed");
                m_CanBeExecuted = true;
            }
            if (m_FearHp < 0)
            {
                m_FearHp = 0;
            }
            Debug.Log(gameObject.name);
            Debug.Log(m_FearHp);      
    }



    public void SetFearEffect()
    {
        m_ICanBeExecuted.gameObject.SetActive(true);
    }
}
