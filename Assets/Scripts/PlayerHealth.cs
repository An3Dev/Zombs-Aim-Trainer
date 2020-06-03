using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using ExitGames.Client.Photon;
public class PlayerHealth : MonoBehaviour, IDamageable<float, GameObject>
{
    public float currentHealth;
    public float currentShields;
    float maxHealth = 100;

    float maxShields = 100;

    SpriteRenderer spriteRenderer;
    PhotonView photonView;

    Slider healthSlider, shieldSlider;

    bool actedOnDeath = false;

    Movement movement;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        currentShields = maxShields;
        photonView = GetComponent<PhotonView>();
        photonView.RPC("StartSpawn", RpcTarget.AllBuffered, 0.5f);

        shieldSlider = GameObject.Find("ShieldSlider").GetComponent<Slider>();
        healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        //Spawn(0);

        movement = transform.root.GetComponent<Movement>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //Damage(1f, transform.gameObject);
        //if (Died() && !actedOnDeath)
        //{
        //    Debug.Log("Died in update");
        //}
    }

    [PunRPC]
    void StartSpawn(float time)
    {
        StartCoroutine("Spawn", time);
    }

    void RefreshSliders()
    {
        shieldSlider.value = currentShields / maxShields;
        healthSlider.value = currentHealth / maxHealth;
        
    }

    IEnumerator Spawn(float time)
    {
        yield return new WaitForSeconds(time);
        currentHealth = maxHealth;
        currentShields = maxShields;

        if(photonView.IsMine)
        {
            RefreshSliders();
            movement.SetAmmo(true, 0, 0);
        }

        transform.GetComponent<Movement>().enabled = true;

        
        if (!PhotonNetwork.OfflineMode)
        {
            photonView.RPC("MakeVisible", RpcTarget.AllBuffered, true, photonView.ViewID);
            transform.position = (Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()));
        }
        else
        {
            MakeVisibleByTransform(true, transform);
            transform.position = Vector3.left * 8;
        }

        actedOnDeath = false;

        StopCoroutine("Spawn");
    }

    public void Damage(float damageTaken, GameObject damager)
    {
        if (currentShields <= 0)
        {
            currentHealth -= damageTaken;
        } else
        {
            currentShields -= damageTaken;
        }

        if (photonView.IsMine)
        {
            RefreshSliders();
        }

        if (Died())
        {
            if(damager.GetComponent<PhotonView>().IsMine && !An3Apps.GameManager.testMode)
            {
                damager.SendMessage("IncreaseKills", 1);
            }

            object deaths;
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(QuickBuildsGame.PLAYER_DEATHS, out deaths))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { QuickBuildsGame.PLAYER_DEATHS, (int)deaths + 1 } });
                Debug.Log("Deaths: " + deaths);
            }
        }

    }

    private bool Died()
    {
        if (currentHealth <= 0 && !actedOnDeath)
        {
            actedOnDeath = true;
            Debug.Log(transform.name + " died");
            if (!PhotonNetwork.OfflineMode)
            {
                photonView.RPC("Die", RpcTarget.AllBuffered);

            } else
            {
                Die();
            }
            return true;
        }
        return false;
    }

    [PunRPC]
    void MakeVisible(bool makeVisible, int viewID)
    {
        Transform thisTransform = PhotonNetwork.GetPhotonView(viewID).transform;
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

        if (!PhotonNetwork.OfflineMode)
        {
            photonView.RPC("MakeVisible", RpcTarget.AllBuffered, false, photonView.ViewID);

            transform.root.GetComponent<Movement>().enabled = false;

            photonView.RPC("StartSpawn", RpcTarget.AllBuffered, 3f);
        } else
        {
            MakeVisibleByTransform(false, transform);
            transform.root.GetComponent<Movement>().enabled = false;

            StartSpawn(3);
        }
        
    }
}
