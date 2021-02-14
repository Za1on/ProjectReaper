using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPromp : MonoBehaviour
{
    public GameObject theMessage;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == ("Player"))
        {
            theMessage.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == ("Player"))
        {
            theMessage.SetActive(false);
        }
    }
}
