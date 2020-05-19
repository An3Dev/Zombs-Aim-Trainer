using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Shoot : MonoBehaviour
{

    public Transform bulletOrigin;
    public GameObject bulletPrefab;

    Camera mainCamera;

    float lastTimeShot;

    public float shootRate = 0.2f;

    public int timesShot;

    PhotonView photonView;

    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        photonView = transform.root.GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TryShooting()
    {
        if (Time.timeSinceLevelLoad - lastTimeShot > shootRate)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
            Vector3 direction = (mouseWorldPos - transform.root.position).normalized;
            
            photonView.RPC("SpawnBullet", RpcTarget.AllViaServer, bulletOrigin.position, direction);

            lastTimeShot = Time.timeSinceLevelLoad;
            timesShot++;

        }
    } 

    [PunRPC]
    public void SpawnBullet(Vector3 location, Vector3 direction)
    {
        //Debug.Log("Info: " + info);
        GameObject bullet = Instantiate(bulletPrefab, location, Quaternion.identity);
        bullet.transform.up = direction;
    }
}
