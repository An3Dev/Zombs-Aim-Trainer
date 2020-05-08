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
            } catch
            {
                // do nothing if object doesn't have damage component
            }
        }
        GameObject effect = Instantiate(effectPrefab, collisionPoint, Quaternion.identity);
        Destroy(effect, 3);
        Destroy(gameObject);
        
    }
}
