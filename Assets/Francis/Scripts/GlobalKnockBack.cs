using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalKnockBack : MonoBehaviour
{

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
          var playerControl = collision.GetComponent<PlayerController>();
            playerControl.m_PlayerKnockbackCount = playerControl.m_KnockbackDuration;
            playerControl.m_GotHit = true;
            if(collision.transform.position.x < transform.position.x)
            {
                playerControl.m_PlayerKBRight = true;
            }
            else
            {
                playerControl.m_PlayerKBRight = false;
            }
            int enemyDamage = this.GetComponent<EnemyManager>().m_EnemyDamage;
            playerControl.GetDamage(enemyDamage);
        }
    }
}
