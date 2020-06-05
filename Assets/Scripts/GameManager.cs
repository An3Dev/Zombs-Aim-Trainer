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

        public static bool testMode = false;

        [SerializeField] GameObject map;

        [SerializeField] TextMeshProUGUI pingText;

        [SerializeField] GameObject spawnPointsContainer;

        bool connected = false;
        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            
        }

        private void Start()
        {
            if (testMode)
            {
                PhotonNetwork.OfflineMode = true;
                //Instantiate(Resources.Load("Player"), Vector3.left * 8, Quaternion.identity);
                SpawnPlayer(Random.Range(0, 3));
            }
            else if (!PhotonNetwork.IsConnectedAndReady)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }

            if (PhotonNetwork.IsConnectedAndReady && !testMode)
            {
                connected = true;
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("SpawnPlayer", RpcTarget.AllBuffered, Random.Range(0, 3));
                    Debug.Log("Test");
                }
            }
        }

        [PunRPC]
        public void SpawnPlayer(int shift)
        {
            Vector3 position = PositionPlayer(shift);

            if (testMode)
            {
                Instantiate(Resources.Load("Player"), position, Quaternion.identity);
            } else 
            {
                PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
                Debug.Log("Instantiated");
            }
        }

        public Vector3 PositionPlayer(int shift)
        {
            int index = !PhotonNetwork.OfflineMode ? PhotonNetwork.LocalPlayer.ActorNumber : 0;

            index += shift;

            if (index > spawnPointsContainer.transform.childCount - 1)
            {
                index = 0;
            }
            Vector3 position = spawnPointsContainer.transform.GetChild(index).position;
            return position;
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

