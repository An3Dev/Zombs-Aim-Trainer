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
        try 
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        catch
        {
            Debug.Log("Null player");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.GetComponent<PhotonView>().IsMine && !An3Apps.GameManager.testMode || PhotonNetwork.OfflineMode)
                {
                    target = player.GetComponent<PhotonView>().transform;
                    break;
                }
            }
        }
        if (target != null)
        transform.position = target.position + transform.forward * -1;
    }
}
