using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class EnemyTime : MonoBehaviour
{
    EnemyTime m_Instance;

    public float DeltaTime;

    public float CustomTimeScale = 1.0f;

    //Make this a singleton
    void start()
    {
        if (m_Instance = null) 
        {
            m_Instance = this;
        }
    }

    void update()
    {
        DeltaTime = Time.deltaTime * CustomTimeScale;
    }
}
