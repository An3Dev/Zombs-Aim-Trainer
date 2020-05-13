using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float speed;

    public GameObject effectPrefab;
    float damage = 25;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Physics2D.queriesHitTriggers = false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 0.2f * Time.deltaTime);
        Physics2D.queriesHitTriggers = true;
        if (hit.collider != null)
        {
            HitSomething(hit.collider.gameObject, hit.point);
        }
        Debug.DrawRay(transform.position, transform.up);

        transform.position += transform.up * speed * Time.deltaTime;
    }


    void HitSomething(GameObject collider, Vector2 collisionPoint)
    {
        if (collider.CompareTag("Target"))
        {
            Destroy(collider);
            SpawnTargets.Instance.DestroyedTarget();
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Destroyable"))
        {
            try
            {
                collider.transform.root.GetComponent<IDamageable<float>>().Damage(damage);
            }
            catch
            {
                // do nothing if object doesn't have damage component
            }
        }
        GameObject effect = Instantiate(effectPrefab, collisionPoint, Quaternion.identity);
        Destroy(effect, 3);
        Destroy(gameObject);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.transform.root.CompareTag("Destroyable"))
        {
            try
            {
                collision.collider.transform.root.GetComponent<IDamageable<float>>().Damage(damage);
            }
            catch
            {
                // do nothing if object doesn't have damage component
            }
        }
    }
}

//using Photon.Realtime;
//using UnityEngine;

//public class Bullet : MonoBehaviour
//{
//    public Player Owner { get; private set; }

//    public float speed;

//    public GameObject effectPrefab;
//    public float damage = 25;

//    public void Start()
//    {
//        Destroy(gameObject, 3.0f);
//    }

//    public void OnCollisionEnter(Collision collision)
//    {
//        Destroy(gameObject);
//    }

//    public void InitializeBullet(Player owner, Vector3 originalDirection, float lag)
//    {
//        Owner = owner;

//        transform.forward = originalDirection;

//        Rigidbody rigidbody = GetComponent<Rigidbody>();
//        rigidbody.velocity = originalDirection * speed;
//        rigidbody.position += rigidbody.velocity * lag;
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        collision.collider.transform.root.GetComponent<IDamageable<float>>().Damage(damage);
//    }
//}

