using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxManager : MonoBehaviour
{
    public GameObject m_PlayerReference;
    public Transform m_PlayerAttackHitBox;
    public Transform m_RightHitbox;
    public Transform m_LeftHitbox;


    public void ManageHitBox(bool isRight)
    {
        if(isRight)
        {
            m_PlayerAttackHitBox = m_RightHitbox;
        }
        else
        {
            m_PlayerAttackHitBox = m_LeftHitbox;
        }    
    }    
}
