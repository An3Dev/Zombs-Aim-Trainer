using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Bullet : MonoBehaviour
{

    public float speed;

    public GameObject effectPrefab;
    float damage = 25;

    PhotonView bulletShooter;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Physics2D.queriesHitTriggers = false;
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 0.05f);
        //Physics2D.queriesHitTriggers = true;
        //if (hit.collider != null && hit.collider.transform.root != transform.root)
        //{
        //    Debug.Log(hit.collider);
        //    HitSomething(hit.collider.gameObject, hit.point);
        //}
        //Debug.DrawRay(transform.position, transform.up);

        transform.position += transform.up * speed * Time.deltaTime;
    }

    public void LateUpdate()
    {
        Physics2D.queriesHitTriggers = false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 0.05f);
        Physics2D.queriesHitTriggers = true;
        if (hit.collider != null && hit.collider.transform.root != transform.root)
        {
            HitSomething(hit.collider.gameObject, hit.point);
        }
        Debug.DrawRay(transform.position, transform.up);
    }

    void HitSomething(GameObject collider, Vector2 collisionPoint)
    {
        if (collider.CompareTag("Target"))
        {
            Destroy(collider);
            SpawnTargets.Instance.DestroyedTarget();
        }

        if (collider.transform.root.GetComponent<IDamageable<float, GameObject>>() != null && bulletShooter.ViewID != collider.transform.root.GetComponent<PhotonView>().ViewID)
        {
            Debug.Log("Collider ViewID: " + collider.transform.root.GetComponent<PhotonView>().ViewID + " Bullet Shooter View ID: " + bulletShooter.ViewID);
            try
            {
                collider.transform.root.GetComponent<IDamageable<float, GameObject>>().Damage(damage, bulletShooter.gameObject);

                //if (bulletShooter.GetComponent<PhotonView>().IsMine)
                //{
                //    bulletShooter.gameObject.SendMessage("IncreaseKills", 1);
                //    Debug.Log("Killed someone");
                //}             
            }
            catch
            {
                // do nothing if object doesn't have damage component
            }

            GameObject effect = Instantiate(effectPrefab, collisionPoint, Quaternion.identity);
            Destroy(effect, 3);
            Destroy(gameObject);
            return;
        }

        Debug.Log("Wall");
    }

    [PunRPC]
    public void AssignParent(int viewID)
    {
        bulletShooter = PhotonNetwork.GetPhotonView(viewID);
    }
}
