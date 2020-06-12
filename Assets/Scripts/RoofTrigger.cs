using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Photon;
using Photon.Pun;

public class RoofTrigger : MonoBehaviourPun
{

    public GameObject roofContainer;

    public bool playerInside = false;
    public void EnableSpriteRenderers(bool enable)
    {
        for(int i = 0; i < roofContainer.transform.childCount; i++)
        {
            roofContainer.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = enable;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            EnableSpriteRenderers(false);
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player" && collision.GetComponent<PhotonView>().IsMine)
        {
            bool enableRenderers = true;
            playerInside = false;
            for(int i = 0; i < transform.parent.childCount; i++)
            {
                if (transform.parent.GetChild(i).GetComponent<RoofTrigger>().playerInside)
                {
                    enableRenderers = false;
                    Debug.Log("Player is inside " + transform.parent.GetChild(i).name);
                    break;
                }
            }

            if (!enableRenderers)
            {
                playerInside = false;
                return;
            }

            EnableSpriteRenderers(true);
        }
    }
}
