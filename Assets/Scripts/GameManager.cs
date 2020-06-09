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

        public static bool lastPersonStanding = false;

        public int killsToWin = 20;
        Movement movement;
        PlayerHealth playerhealth;
        PhotonView playerPhotonView;

        PhotonView thisPhotonView;

        bool connected = false;

        private Dictionary<int, GameObject> playerListEntries;

        bool startedGame = false;
        GameObject player;

        Player[] playerArray;

        int playersAlive = 4;
        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            thisPhotonView = GetComponent<PhotonView>();

            playerListEntries = new Dictionary<int, GameObject>();

            foreach (Player p in PhotonNetwork.PlayerList)
            {
            //    GameObject entry = Instantiate(PlayerOverviewEntryPrefab);
            //    entry.transform.SetParent(gameObject.transform);
            //    entry.transform.localScale = Vector3.one;
            //    //entry.GetComponent<Text>().color = QuickBuildsGame.GetColor(p.GetPlayerNumber());
            //    entry.GetComponent<Text>().text = string.Format("{0}\nElims: {1}\nDeaths: 0", p.NickName, p.GetScore());

                playerListEntries.Add(p.ActorNumber, entry);
            }
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
                    thisPhotonView.RPC("SpawnPlayer", RpcTarget.AllBuffered, Random.Range(0, 3));
                }
                playersAlive = PhotonNetwork.PlayerList.Length;
                playerArray = PhotonNetwork.PlayerList;

            }
        }

        [PunRPC]
        public void PlayerDied(int photonViewID)
        {
            playersAlive--;
            Debug.Log("Player died");
        }

        [PunRPC]
        public void SpawnPlayer(int shift)
        {
            Vector3 position = PositionPlayer(shift);

            if (testMode)
            {
                player = Instantiate(Resources.Load("Player"), position, Quaternion.identity) as GameObject;
            } else 
            {
                player = PhotonNetwork.Instantiate("Player", position, Quaternion.identity) as GameObject;
            }
            playerhealth = player.GetComponent<PlayerHealth>();
            movement = player.GetComponent<Movement>();
            playerPhotonView = player.GetComponent<PhotonView>();
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

            if (playersAlive <= 1 && PhotonNetwork.IsMasterClient && !startedGame && lastPersonStanding)
            {
                thisPhotonView.RPC("RestartGame", RpcTarget.AllBuffered, Random.Range(0, 3));
                startedGame = true;
                Debug.Log(playerPhotonView.transform);
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            GameObject entry;
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
            {
                //entry.GetComponent<Text>().text = string.Format("{0}\nElims: {1}\nDeaths: {2}", targetPlayer.NickName, targetPlayer.GetScore(), targetPlayer.CustomProperties[QuickBuildsGame.PLAYER_DEATHS]);
                if (targetPlayer.GetScore() >= killsToWin)
                {
                    Debug.Log("Target player: " + targetPlayer);
                }
            }
        }

        [PunRPC]
        void RestartGame(int shift)
        {
            if (!PhotonNetwork.OfflineMode && PhotonNetwork.IsMasterClient)
            {
                PositionPlayer(shift);
            }
            if (playerPhotonView.IsMine)
            {
                player.GetComponent<Movement>().SetAmmo(true, 0, 0);
                player.GetComponent<PlayerHealth>().ReplenishHealth(200, 2);
                startedGame = false;
                playerPhotonView.RPC("StartSpawn", RpcTarget.AllBuffered, 3f);
                Debug.Log("Restart");
            }
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

