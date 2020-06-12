using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite topViewSprite;
    public Sprite sideViewSprite;
    public Sprite muzzleFlashSprite;
    public Sprite bulletSprite;

    public float timeBetweenShots;
    public float damage;
    public float reloadTime;
    public int magazineSize;
    public int bloomAmount;
    public float bulletSpeed;
    public float fov;

    public int bulletsShotAtOnce;
    public float timeBeforeDestroy;

    public int startingAmmo;

    public bool throwable;

    public bool isHealingItem;
    public float healingTime;
    public float healingAmount;
    public enum typeOfHeal { Health, Shields, Both}
}
