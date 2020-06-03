using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
namespace An3Apps 
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;

        public static bool testMode = true;

        [SerializeField] GameObject map;

        [SerializeField] TextMeshProUGUI pingText;

        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            if (testMode)
            {
                PhotonNetwork.OfflineMode = true;
                Instantiate(Resources.Load("Player"), Vector3.left * 8, Quaternion.identity);
            }
            else if (!PhotonNetwork.IsConnectedAndReady)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }

            if (PhotonNetwork.IsConnected && !testMode)
            {
                PhotonNetwork.Instantiate("Player", Vector3.left * 8 + (Vector3.right * PhotonNetwork.CurrentRoom.PlayerCount * PhotonNetwork.LocalPlayer.GetPlayerNumber()), Quaternion.identity);

                // Replace each map item with a photon network item.
            }
        }



        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Escape) && PhotonNetwork.IsMasterClient)
            {
                // show restart game options, 
            }
            if (Input.GetKey(KeyCode.Escape))
            {

                if (PhotonNetwork.IsConnectedAndReady)
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    StopAllCoroutines();
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");

                    PhotonNetwork.LeaveRoom();
                    PhotonNetwork.Disconnect();
                }
            }
            pingText.text = PhotonNetwork.GetPing() + "ms";
        }

        [PunRPC]
        void RestartGame()
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

            }
            else
            {
                text.text = "Click Again";
            }
        }

        public void OnDisconnectedFromServer()
        {
            SceneManager.Instance.LoadScene("Lobby");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }
    }
}

