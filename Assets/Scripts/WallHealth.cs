using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHealth : MonoBehaviour, IDamageable<float>
{
    public enum WallType { Wood, Brick, Metal, Electric }

    public WallType wallType = WallType.Wood;

    int[] wallHealth = { 100, 150, 300, 150 };

    int maxHealth;

    int currentHealth;

    private void Awake()
    {
        int toInt = (int)wallType;
        maxHealth = wallHealth[toInt];
        currentHealth = maxHealth;
        Debug.Log(currentHealth);
    }

    public void Damage(float damageTaken)
    {
        currentHealth -= Mathf.FloorToInt(damageTaken);
        CheckHealth();
        ChangeWallAppearance(currentHealth);
    }

    void ChangeWallAppearance(float health)
    {
        foreach(Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
                Color color = renderer.color;
                color.a = health / maxHealth;
                renderer.color = color;
            }
        }
    }
    void CheckHealth()
    {
        if(currentHealth <= 0)
        {
            Debug.Log(transform.name + " was destroyed");
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
