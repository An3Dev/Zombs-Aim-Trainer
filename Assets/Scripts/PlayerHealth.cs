using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class PlayerHealth : MonoBehaviour, IDamageable<float, GameObject>
{
    private float currentHealth;
    
    public float maxHealth = 200;

    SpriteRenderer spriteRenderer;
    PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonView.RPC("StartSpawn", RpcTarget.AllBufferedViaServer, 0.5f);

        //Spawn(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void StartSpawn(float time)
    {
        StartCoroutine("Spawn", time);
        Debug.Log("Started spawn");
    }


    IEnumerator Spawn(float time)
    {
        yield return new WaitForSeconds(time);

        Debug.Log("Spawn");
        currentHealth = maxHealth;

        if (photonView.IsMine)
        {
            transform.GetComponent<Movement>().enabled = true;
        }

        transform.position = (Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()));
        photonView.RPC("MakeVisible", RpcTarget.AllBufferedViaServer, true);
        StopCoroutine("Spawn");
    }

    public void Damage(float damageTaken, GameObject damager)
    {
        Debug.Log("Current Health: " + currentHealth + " Max Health: " + maxHealth);

        currentHealth -= damageTaken;
        Debug.Log("Damage Taken: " + damageTaken);
        Debug.Log("Current Health: " + currentHealth);

        if (Died())
        {
            if(damager.GetComponent<PhotonView>().IsMine)
            {
                damager.SendMessage("IncreaseKills", 1);
            }
        }

    }

    private bool Died()
    {
        if (currentHealth <= 0)
        {
            // is dead = true
            Debug.Log(transform.name + " died");
            photonView.RPC("Die", RpcTarget.AllBufferedViaServer);
            return true;
        }
        return false;
    }

    [PunRPC]
    void MakeVisible(bool makeVisible)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            // if the child has a sprite renderer and its state is not what it should be
            if (transform.GetChild(i).GetComponent<SpriteRenderer>() != null && transform.GetChild(i).GetComponent<SpriteRenderer>().enabled == !makeVisible)
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = makeVisible;
            }
        }
        transform.GetComponent<SpriteRenderer>().enabled = makeVisible;
    }

    [PunRPC]
    void Die()
    {
        photonView.RPC("MakeVisible", RpcTarget.AllBufferedViaServer, false);
        //MakeVisible(false);
        if (photonView.IsMine)
        {
            try
            {
                transform.root.GetComponent<Movement>().enabled = false;
            }
            catch
            {
                Debug.Log("no Movement");
            }
        }

        photonView.RPC("StartSpawn", RpcTarget.AllBufferedViaServer, 3f);
    }
}
