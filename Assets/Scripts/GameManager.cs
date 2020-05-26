using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

namespace An3Apps 
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
        // Start is called before the first frame update
        void Start()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Instantiate("Player", Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()), Quaternion.identity);
            }

            Instance = this;
        }


        // Update is called once per frame
        void Update()
        {

        }

        [PunRPC]
        public void EnableGameObject(bool setActive, int photonID)
        {
            PhotonView Disable = PhotonView.Find(photonID);
            Disable.transform.gameObject.SetActive(setActive);
            Debug.Log("SetActive: " + setActive + " for " + Disable.transform.name);
        }
    }

}

