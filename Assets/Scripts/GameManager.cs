using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

namespace An3Apps 
{
    public class GameManager : MonoBehaviour
    {

        
        // Start is called before the first frame update
        void Start()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Instantiate("Player", Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()), Quaternion.identity);
            }
        }


        // Update is called once per frame
        void Update()
        {

        }
    }

}

