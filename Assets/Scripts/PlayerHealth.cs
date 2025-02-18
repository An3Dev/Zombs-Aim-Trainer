﻿using An3Apps;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField] GameObject damageTextPrefab;
    [SerializeField] TextMeshPro damageText;

    [SerializeField]
    GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        currentShields = maxShields;
        photonView = GetComponent<PhotonView>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (!PhotonNetwork.OfflineMode)
        {
            //photonView.RPC("StartSpawn", RpcTarget.AllBuffered, 0f);
        }
        else
        {
            //StartSpawn(0);
        }

        shieldSlider = GameObject.Find("ShieldSlider").GetComponent<Slider>();
        healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        //Spawn(0);

        movement = transform.root.GetComponent<Movement>();
        SetVariables();
    }

    public bool ReplenishHealth(float amount, int healthShieldsOrBoth)
    {
        Debug.Log("Replenish");
        if (healthShieldsOrBoth == 2)
        {
            currentShields = Mathf.Clamp(currentHealth + amount - maxHealth, 0, maxShields);

            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        } else if (healthShieldsOrBoth == 1)
        {
            if (currentShields == maxHealth)
            {
                return false;
            }
            currentShields = Mathf.Clamp(currentShields + amount, 0, maxShields);
        } else if (healthShieldsOrBoth == 0)
        {
            if (currentHealth == maxHealth)
            {
                return false;
            }
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        }

        RefreshSliders();
        return true;
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

        gameManager.DisableLeaderboardScreen();
        SetVariables();
        transform.GetComponent<Movement>().enabled = true;


        if (!PhotonNetwork.OfflineMode)
        {
            photonView.RPC("MakeVisible", RpcTarget.AllBuffered, true, photonView.ViewID);
            //transform.position = (Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()));
            transform.position = gameManager.PositionPlayer(Random.Range(0, 3));
        }
        else
        {
            MakeVisibleByTransform(true, transform);
            transform.position = gameManager.PositionPlayer(Random.Range(0, 3));
        }

        actedOnDeath = false;

        StopCoroutine("Spawn");
    }

    void SetVariables()
    {
        currentHealth = maxHealth;
        currentShields = maxShields;

        if (photonView.IsMine)
        {
            RefreshSliders();
            movement.SetAmmo(true, 0, 0);
        }
    }

    public void Damage(float damageTaken, GameObject damager)
    {
        bool shields = true;

        if (currentShields <= 0)
        {
            currentHealth -= damageTaken;
            shields = false;
        }
        else
        {
            // if damage penetrates the shield and goes to health
            if (currentShields - damageTaken < 0)
            {
                currentHealth -= damageTaken - currentShields;
                currentShields = 0;
            }
            else
            {
                currentShields -= damageTaken;
            }
            shields = true;
        }
        if (!photonView.IsMine)
        {
            GameObject damageTextGO = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);
            TextMeshPro text = damageTextGO.GetComponent<TextMeshPro>();
            text.text = damageTaken.ToString();

            if (shields)
            {
                text.color = Color.blue;
            } else
            {
                text.color = Color.white;
            }
            Vector3 randomDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
            damageTextGO.transform.LeanMove(transform.position + randomDirection * 2, 1);

            Destroy(damageTextGO, 1);
        }

        if (photonView.IsMine)
        {
            RefreshSliders();
        }

        if (Died())
        {
            if (damager.GetComponent<PhotonView>().IsMine && !An3Apps.GameManager.testMode)
            {
                damager.transform.root.GetComponent<Movement>().IncreaseKills(1, photonView.Owner);
            }
            AddDeath(photonView.Owner, 1);
        }
    }

    public static void SetDeaths(Player player, int newDeaths)
    {
        ExitGames.Client.Photon.Hashtable deaths = new ExitGames.Client.Photon.Hashtable();
        deaths[PunPlayerScores.PlayerScoreProp] = newDeaths;

        player.SetCustomProperties(deaths);  // this locally sets the score and will sync it in-game asap.
    }

    public static int GetPlayerDeaths(Player player)
    {
        object deaths;
        if (player.CustomProperties.TryGetValue(QuickBuildsGame.PLAYER_DEATHS, out deaths))
        {
            return (int)deaths;
        }
        SetDeaths(player, 0);
        return 0;
    }

    public static void AddDeath(Player player, int scoreToAddToCurrent)
    {
        int current = GetPlayerDeaths(player);
        current = current + scoreToAddToCurrent;

        ExitGames.Client.Photon.Hashtable deaths = new ExitGames.Client.Photon.Hashtable();  // using PUN's implementation of Hashtable
        deaths[QuickBuildsGame.PLAYER_DEATHS] = current;

        player.SetCustomProperties(deaths);  // this locally sets the score and will sync it in-game asap.
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
            }
            else
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

            // Disable collider

            // tell game manager that this player died
            gameManager.photonView.RPC("PlayerDied", RpcTarget.AllBuffered, photonView.ViewID);

            // if the game is not last man standing then respawn
            if (!GameManager.lastPersonStanding)
            {
                photonView.RPC("StartSpawn", RpcTarget.AllBuffered, 3f);
            }
        }
        else
        {
            MakeVisibleByTransform(false, transform);
            transform.root.GetComponent<Movement>().enabled = false;

            StartSpawn(3);
        }
    }
}
