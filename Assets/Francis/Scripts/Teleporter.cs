using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public bool m_IsTeleporting;


    public virtual void Start()
    {
        m_IsTeleporting = false;
    }
}
