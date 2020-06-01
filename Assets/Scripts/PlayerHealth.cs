using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class PlayerHealth : MonoBehaviour, IDamageable<float, GameObject>
{
    public float currentHealth;
    
    public float maxHealth = 200;

    SpriteRenderer spriteRenderer;
    PhotonView photonView;

    bool actedOnDeath = false;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        photonView = GetComponent<PhotonView>();
        photonView.RPC("StartSpawn", RpcTarget.AllBuffered, 0.5f);

        //Spawn(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Died() && !actedOnDeath)
        {
            Debug.Log("Died in update");
        }
    }

    [PunRPC]
    void StartSpawn(float time)
    {
        StartCoroutine("Spawn", time);
        Debug.Log("Started spawn");
    }


    IEnumerator Spawn(float time)
    {
        Debug.Log(Time.time);
        yield return new WaitForSeconds(time);
        Debug.Log(Time.time);

        Debug.Log("Spawn");
        currentHealth = maxHealth;

        if (!An3Apps.GameManager.testMode)
        {
            transform.GetComponent<Movement>().enabled = true;
        }

        transform.position = (Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()));
        photonView.RPC("MakeVisible", RpcTarget.AllBuffered, true, photonView.ViewID);
        StopCoroutine("Spawn");
        actedOnDeath = false;
    }

    public void Damage(float damageTaken, GameObject damager)
    {
        Debug.Log("Current Health: " + currentHealth + " Max Health: " + maxHealth);

        currentHealth -= damageTaken;
        Debug.Log("Damage Taken: " + damageTaken);
        Debug.Log("Current Health: " + currentHealth);

        if (Died())
        {
            if(damager.GetComponent<PhotonView>().IsMine && !An3Apps.GameManager.testMode)
            {
                damager.SendMessage("IncreaseKills", 1);
            }
        }

    }

    private bool Died()
    {
        if (currentHealth <= 0 && !actedOnDeath)
        {
            actedOnDeath = true;
            Debug.Log(transform.name + " died");
            photonView.RPC("Die", RpcTarget.AllBuffered);
            return true;
        }
        return false;
    }

    [PunRPC]
    void MakeVisible(bool makeVisible, int viewID)
    {
        Transform thisTransform = PhotonNetwork.GetPhotonView(viewID).transform;
        Debug.Log("Make visible: " + makeVisible);
        for(int i = 0; i < thisTransform.childCount; i++)
        {
            //Debug.Log("Child count: " + thisTransform.childCount);
            // if the child has a sprite renderer and its state is not what it should be
            if (thisTransform.GetChild(i).GetComponent<SpriteRenderer>() != null && thisTransform.GetChild(i).GetComponent<SpriteRenderer>().enabled == !makeVisible)
            {
                thisTransform.GetChild(i).GetComponent<SpriteRenderer>().enabled = makeVisible;
                //Debug.Log("Disabled: " + thisTransform.GetChild(i).GetComponent<SpriteRenderer>());
            }

            if (thisTransform.childCount > 0)
            {
                MakeVisibleByTransform(makeVisible, thisTransform.GetChild(i));
            }
        }
        //transform.GetComponentInChildren<SpriteRenderer>().enabled = makeVisible;
    }

    void MakeVisibleByTransform(bool makeVisible, Transform thisTransform)
    {
        //Debug.Log("Make visible: " + makeVisible);
        for (int i = 0; i < thisTransform.childCount; i++)
        {
            //Debug.Log("Child count: " + thisTransform.childCount);
            // if the child has a sprite renderer and its state is not what it should be
            if (thisTransform.GetChild(i).GetComponent<SpriteRenderer>() != null && thisTransform.GetChild(i).GetComponent<SpriteRenderer>().enabled == !makeVisible)
            {
                thisTransform.GetChild(i).GetComponent<SpriteRenderer>().enabled = makeVisible;
                //Debug.Log("Disabled: " + thisTransform.GetChild(i).GetComponent<SpriteRenderer>());
            }

            if (thisTransform.childCount > 0)
            {
                MakeVisibleByTransform(makeVisible, thisTransform.GetChild(i));
            }
        }
        //transform.GetComponentInChildren<SpriteRenderer>().enabled = makeVisible;
    }

    [PunRPC]
    void Die()
    {
        photonView.RPC("MakeVisible", RpcTarget.AllBuffered, false, photonView.ViewID);
        //MakeVisible(false);
        //if (photonView.IsMine)
        //{
        //    try
        //    {
        transform.root.GetComponent<Movement>().enabled = false;
        //    }
        //    catch
        //    {
        //        Debug.Log("no Movement");
        //    }
        //}

        photonView.RPC("StartSpawn", RpcTarget.AllBuffered, 3f);
        //Debug.Log("MakeInvisible");
    }
}
