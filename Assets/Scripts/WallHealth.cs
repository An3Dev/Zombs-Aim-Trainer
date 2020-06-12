using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using An3Apps;

public class WallHealth : MonoBehaviour, IDamageable<float, GameObject>
{
    public enum WallType { Wood, Brick, Metal, Electric, SuperStrong }

    public WallType wallType = WallType.Wood;

    int[] wallHealth = { 100, 150, 250, 100, 1500 };

    int maxHealth;

    int currentHealth;

    PhotonView photonView;

    bool hasPhotonView = true;
    float minimumTransparency = 0.1f;

    PhotonView gameManagerPhotonView;

    private void Awake()
    {
        int toInt = (int)wallType;
        maxHealth = wallHealth[toInt];
        currentHealth = maxHealth;
        if (GetComponent<PhotonView>() != null)
        {
            photonView = GetComponent<PhotonView>();
        } else
        {
            photonView = null;
            hasPhotonView = false;
            Debug.Log("No photon view");
        }

        gameManagerPhotonView = GameObject.Find("GameManager").GetComponent<PhotonView>();
    }

    public void Damage(float damageTaken, GameObject manager)
    {
        currentHealth -= Mathf.FloorToInt(damageTaken);
        //Debug.Log(transform.name + " health: " + currentHealth);
        CheckHealth();
        ChangeWallAppearance(currentHealth, transform);
    }

    void ChangeWallAppearance(float health, Transform parent)
    {
        try
        {
            if (parent.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer renderer = parent.GetComponent<SpriteRenderer>();
                Color color = renderer.color;
                color.a = (health / maxHealth) + minimumTransparency;
                renderer.color = color;
            }
        }
        catch
        {
            Debug.Log("error changing wall");
        }

        foreach (Transform child in transform)
        {
            if (child.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
                Color color = renderer.color;
                color.a = (health / maxHealth) + minimumTransparency;
                renderer.color = color;
            }

            if (child.childCount > 0)
            {
                ChangeWallAppearance(health, child);
            }
        }
    }
    void CheckHealth()
    {
        if(currentHealth <= 0)
        {
            //Destroy(gameObject);

            //photonView.gameObject.SetActive(false);

            Debug.Log("Has photon view? " + hasPhotonView);

            if (!PhotonNetwork.OfflineMode && hasPhotonView)
            {
                Debug.Log("DestroySelf");

                photonView.RPC("DestroySelf", RpcTarget.AllBuffered);
                
            } else if (!PhotonNetwork.OfflineMode && !hasPhotonView)
            {
                Debug.Log("Destroy");
                Destroy(gameObject);
                gameManagerPhotonView.RPC("DisableGameObject", RpcTarget.AllBuffered, transform.name, transform.parent);
                //Destroy(transform.root.gameObject);
            }
        }
    }

    [PunRPC]
    void DestroySelf()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
