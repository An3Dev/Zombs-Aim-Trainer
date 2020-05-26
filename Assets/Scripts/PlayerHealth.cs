using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerHealth : MonoBehaviour, IDamageable<float, GameObject>
{
    private float currentHealth;
    
    public float maxHealth = 200;

    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        Spawn(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerable Spawn(float time)
    {
        yield return new WaitForSeconds(time);

        currentHealth = maxHealth;

        transform.GetComponent<Movement>().enabled = true;

        transform.position = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5));
        MakeVisible(true);
    }

    public void Damage(float damageTaken, GameObject damager)
    {
        currentHealth -= Mathf.FloorToInt(damageTaken);
        CheckHealth();
       
        damager.SendMessage("IncreaseKills", 1);
        
    }

    private void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            // is dead = true
            Debug.Log(transform.name + " died");
            transform.root.GetComponent<SpriteRenderer>().enabled = false;
            Die();
        }
    }

    void MakeVisible(bool makeVisible)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = !makeVisible)
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = makeVisible;
            }
        }
        transform.GetComponent<SpriteRenderer>().enabled = false;
    }

    void Die()
    {
        MakeVisible(false);
        transform.GetComponent<Movement>().enabled = false;
        Spawn(5);
    }
}
