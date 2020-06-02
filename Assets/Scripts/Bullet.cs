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
    Transform bulletShooterTransform;
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

    public void SetStats(float setSpeed, float setDamage, float timeBeforeDestroyed)
    {
        speed = setSpeed;
        damage = setDamage;

        Destroy(gameObject, timeBeforeDestroyed);
    }

    public void LateUpdate()
    {
        Physics2D.queriesHitTriggers = false;
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 0.1f);
        RaycastHit2D hit = Physics2D.CapsuleCast(transform.position, new Vector3(0.3f, 0.3f, 0.3f), CapsuleDirection2D.Vertical, 360, transform.up, 0.2f);
        Physics2D.queriesHitTriggers = true;

        if (hit.collider != null && hit.collider.transform.root != transform.root)
        {
            HitSomething(hit.collider.gameObject, hit.point);
        }
        Debug.DrawRay(transform.position, transform.up);
    }

    public void OnDestroy()
    {

    }

    void HitSomething(GameObject collider, Vector2 collisionPoint)
    {
        if (collider.CompareTag("Target"))
        {
            Destroy(collider);
            SpawnTargets.Instance.DestroyedTarget();
        }

        // if the collider has a damageable script, and if bullet isn't hitting itself.
        if (collider.transform.root.GetComponent<IDamageable<float, GameObject>>() != null && (!PhotonNetwork.OfflineMode ? bulletShooter.ViewID != collider.transform.root.GetComponent<PhotonView>().ViewID : bulletShooterTransform.root != collider.transform.root))
        {
            //Debug.Log("Collider ViewID: " + collider.transform.root.GetComponent<PhotonView>().ViewID + " Bullet Shooter View ID: " + bulletShooter.ViewID);
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
                collider.transform.root.GetComponent<IDamageable<float, GameObject>>().Damage(damage, bulletShooterTransform.gameObject);
            }

            GameObject effect = Instantiate(effectPrefab, collisionPoint, Quaternion.identity);
            Destroy(effect, 3);
            Destroy(gameObject);
            return;
        }
    }

    [PunRPC]
    public void AssignParent(int viewID)
    {
        bulletShooter = PhotonNetwork.GetPhotonView(viewID);   
    }

    public void AssignParentTransform(Transform shooter)
    {
        bulletShooterTransform = shooter;
    }
}
