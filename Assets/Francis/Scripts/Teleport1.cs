using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport1 : Teleporter
{

    public PlayerController m_Player;
    public GameObject m_Teleporter2;

    public override void Start()
    {
        m_IsTeleporting = false;
    }

    public void Update()
    {
        if (m_Player.m_IsDashing)
        {
            gameObject.GetComponent<Collider2D>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && m_Player.m_IsDashing)
        {
            if(!m_IsTeleporting)
            {
                StartCoroutine(TeleportOne());
            }
        }
    }

    IEnumerator TeleportOne()
    {
        m_Player.m_PlayerTeleporting = true;
        m_Teleporter2.SetActive(false);
        m_IsTeleporting = true;
        m_Player.transform.position = m_Teleporter2.transform.position;
        yield return new WaitForSeconds(m_Player.m_DashCooldown);
        m_Teleporter2.SetActive(true);
        m_IsTeleporting = false;
        m_Player.m_PlayerTeleporting = false;
    }
}
