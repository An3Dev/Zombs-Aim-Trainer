using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float speed;

    public GameObject effect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, 0.2f * Time.deltaTime);
        
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
        Instantiate(effect, collisionPoint, Quaternion.identity);

        Destroy(gameObject);
        
    }
}
