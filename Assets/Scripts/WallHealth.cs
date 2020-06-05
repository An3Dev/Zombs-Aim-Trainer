using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class WallHealth : MonoBehaviour, IDamageable<float, GameObject>
{
    public enum WallType { Wood, Brick, Metal, Electric, SuperStrong }

    public WallType wallType = WallType.Wood;

    int[] wallHealth = { 100, 150, 300, 100, 1500 };

    int maxHealth;

    int currentHealth;

    PhotonView photonView;

    float minimumTransparency = 0.1f;

    private void Awake()
    {
        int toInt = (int)wallType;
        maxHealth = wallHealth[toInt];
        currentHealth = maxHealth;
        photonView = GetComponent<PhotonView>();
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
            Debug.Log(transform.name + " was destroyed");
            //Destroy(gameObject);

            //photonView.gameObject.SetActive(false);
            if (!PhotonNetwork.OfflineMode)
            {
                
                photonView.RPC("DestroySelf", RpcTarget.AllBuffered);
                
            } else
            {
                Destroy(transform.root.gameObject);
                Debug.Log("Destroy");
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
