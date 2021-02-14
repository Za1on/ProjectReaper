using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportEndLevel : Teleporter
{

    public PlayerController m_Player;
    public GameObject m_SceneManage;
    public GameObject m_TeleporterEnd;

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
        if (collision.tag == "Player" && m_Player.m_IsDashing)
        {
            if (!m_IsTeleporting)
            {
                m_SceneManage.GetComponent<SceneLoader>().PlayedDied();
            }
        }
    }

}
