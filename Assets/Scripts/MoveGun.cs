using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGun : MonoBehaviour
{

    public Transform weapon;
    public Vector3 offset;


    // Start is called before the first frame update
    void Start()
    {
        offset = weapon.transform.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        weapon.position = transform.position + offset;
        weapon.up = transform.up;
    }
}
