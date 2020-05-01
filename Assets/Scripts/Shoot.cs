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

    public static Shoot Instance;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && Time.timeSinceLevelLoad - lastTimeShot > shootRate && SpawnTargets.Instance.startGame)
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
