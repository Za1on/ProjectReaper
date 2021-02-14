using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearPropagation : ExecutionManager
{
    private EnemyManager m_CurrentGO;
    public void StartFearPropagation()
    {
        StartCoroutine(DoScalePropagation());
    }

    private void CheckForTargets()
    {
        Collider2D[] _ExecuteEnemy = Physics2D.OverlapCircleAll(m_FearPropPos.position, m_FearPropCurrentSize, m_PlayerManager.m_EnemyLayers);
        foreach (Collider2D _CurrentEnemy in _ExecuteEnemy)
        {
            if (_CurrentEnemy.isTrigger == true)
            {
                Debug.Log("Fear is propagated");
                m_CurrentGO = _CurrentEnemy.GetComponent<EnemyManager>();
                m_CurrentGO.ReceiveFear(m_CurrentGO.m_FearPropagationDamage);
                if (m_CurrentGO.m_FearHp <= m_CurrentGO.m_FearExecutionThreshold)
                {
                    m_CurrentGO.SetFearEffect();
                    m_CurrentGO.m_CanBeExecuted = true;
                }
            }
        }
    }

    private IEnumerator DoScalePropagation()
    {
        float timer = 0f;

        float lerpPercentage = 0f;

        transform.position = m_PlayerManager.transform.position;


        while (timer < m_FearPropScaleDuration)
        {
            lerpPercentage += Time.deltaTime / m_FearPropScaleDuration;
            m_FearPropCurrentSize = Mathf.Lerp(m_FearPropStartSize, m_FearPropEndSize, lerpPercentage);
            CheckForTargets();
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (m_FearPropPos != null) 
        {
            Gizmos.DrawWireSphere(m_FearPropPos.position, m_FearPropEndSize);
        }
        if (m_FearPropPos != null)
        {
            Gizmos.DrawWireSphere(m_FearPropPos.position, m_FearPropStartSize);
        }
        if (m_FearPropPos != null)
        {
            Gizmos.DrawWireSphere(m_FearPropPos.position, m_FearPropCurrentSize);
        }
    }
}
