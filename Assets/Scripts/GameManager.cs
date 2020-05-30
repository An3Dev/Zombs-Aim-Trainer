using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace An3Apps 
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public static bool testMode = false;

        public void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (testMode)
            {
                PhotonNetwork.OfflineMode = true;
            }

            if (PhotonNetwork.IsConnected && !testMode)
            {
                PhotonNetwork.Instantiate("Player", Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()), Quaternion.identity);
            }
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

        public void OnLeaveMatchPressed(Text text)
        {
            if (text.text == "Click Again!")
            {
                PhotonNetwork.Disconnect();
                PhotonNetwork.LeaveLobby();
                PhotonNetwork.LeaveRoom();

            } else {
                text.text = "Click Again";
            }
        }

        public void OnDisconnectedFromServer()
        {
             SceneManager.Instance.LoadScene("Lobby");
        }
    }
}

