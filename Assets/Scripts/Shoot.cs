using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public Transform bulletOrigin;
    public GameObject bulletPrefab;

    Camera mainCamera;

    float lastTimeShot;

    public float shootRate = 0.2f;

    public int timesShot;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShootWeapon()
    {
        if (Time.timeSinceLevelLoad - lastTimeShot > shootRate)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
            Vector3 direction = (mouseWorldPos - transform.root.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, bulletOrigin.position, Quaternion.identity);
            bullet.transform.up = direction;

            lastTimeShot = Time.timeSinceLevelLoad;
            timesShot++;
        }
    } 
}
