using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PhotonView>().IsMine && !An3Apps.GameManager.testMode)
            {
                target = player.GetComponent<PhotonView>().transform;
                break;
            }
        }

        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.GetComponent<PhotonView>().IsMine && !An3Apps.GameManager.testMode)
                {
                    target = player.GetComponent<PhotonView>().transform;
                    break;
                }
            }
        }
        transform.position = target.position + transform.forward * -1;
    }
}
