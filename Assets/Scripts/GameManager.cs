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

        bool startedGame = false;
        GameObject player;

        Player[] playerArray;

        int playersAlive = 4;

        string winnerName = "";

        [SerializeField] GameObject endScreenPanel;
        [SerializeField] GameObject leaderBoardSlotPrefab;
        [SerializeField] TextMeshProUGUI winnerText;
        [SerializeField] GameObject slotParent;

        public GameObject settingsGameObject;
        Keybinds keybinds;
        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            thisPhotonView = GetComponent<PhotonView>();
            keybinds = FindObjectOfType<Keybinds>();
        }

        public void ImageClick(GameObject gameObject)
        {
            playerPhotonView.transform.GetComponent<Movement>().InventoryClick(gameObject);
        }

        private void Start()
        {
            if (testMode)
            {
                PhotonNetwork.OfflineMode = true;
                //Instantiate(Resources.Load("Player"), Vector3.left * 8, Quaternion.identity);
                SpawnPlayer(Random.Range(0, spawnPointsContainer.transform.childCount - 1));
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
                    thisPhotonView.RPC("SpawnPlayer", RpcTarget.AllBuffered, Random.Range(0, spawnPointsContainer.transform.childCount - 1));
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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // show settings
                if (!settingsGameObject.activeInHierarchy) 
                {
                    settingsGameObject.SetActive(true);
                } else if (settingsGameObject.activeInHierarchy)
                {
                    if(!keybinds)
                    {
                        keybinds = FindObjectOfType<Keybinds>();
                    }
                    // if not changing a key
                    if (!keybinds.currentKey)
                    {
                        settingsGameObject.SetActive(false);
                    } else
                    {
                        // do nothing because user canceled keybind assignment
                    }
                }

            }
            //if (Input.GetKey(KeyCode.Escape))
            //{

            //    if (PhotonNetwork.IsConnectedAndReady)
            //    {
            //        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            //        StopAllCoroutines();
            //        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");

            //        PhotonNetwork.LeaveRoom();
            //        PhotonNetwork.Disconnect();
            //    }
            //}
            pingText.text = PhotonNetwork.GetPing() + "ms";

            if (playersAlive <= 1 && PhotonNetwork.IsMasterClient && !startedGame && lastPersonStanding)
            {
                thisPhotonView.RPC("RestartGame", RpcTarget.AllBuffered, Random.Range(0, spawnPointsContainer.transform.childCount - 1));
                startedGame = true;
                Debug.Log(playerPhotonView.transform);
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            GameObject entry;
            if (PlayerStatsPanel.playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
            {
                //entry.GetComponent<Text>().text = string.Format("{0}\nElims: {1}\nDeaths: {2}", targetPlayer.NickName, targetPlayer.GetScore(), targetPlayer.CustomProperties[QuickBuildsGame.PLAYER_DEATHS]);
                if (targetPlayer.GetScore() >= killsToWin)
                {

                    if (PhotonNetwork.IsMasterClient)
                    {
                        thisPhotonView.RPC("RestartGame", RpcTarget.AllBuffered, Random.Range(0, spawnPointsContainer.transform.childCount - 1));

                        // resets score for all players
                        foreach (Player player in PhotonNetwork.PlayerList)
                        {
                            player.SetScore(0);
                        }
                    }

                    // set the winner of the match
                    ShowEndScreen(targetPlayer.NickName);
                }
            }
        }

        void ShowEndScreen(string winner)
        {
            endScreenPanel.SetActive(true);
            winnerText.text = winner + " Won!";

            Player[] playerList = PhotonNetwork.PlayerList;
            for(int i = 0; i < playerList.Length; i++)
            {
                Player player = playerList[i];
                GameObject leaderboardSlot = Instantiate(leaderBoardSlotPrefab, slotParent.transform);
                Vector3 position = leaderboardSlot.transform.localPosition;
                position.y = -100 - (65 * i);
                Debug.Log(i);
                leaderboardSlot.GetComponent<RectTransform>().anchoredPosition = position;
                Transform itemContainer = leaderboardSlot.transform.Find("Panel");
                
                itemContainer.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = player.NickName;

                float kills = player.GetScore();
                float deaths = PlayerHealth.GetPlayerDeaths(playerPhotonView.Owner);
                itemContainer.transform.Find("KillsText").GetComponent<TextMeshProUGUI>().text = kills.ToString();
                itemContainer.transform.Find("DeathsText").GetComponent<TextMeshProUGUI>().text = deaths.ToString();
                itemContainer.transform.Find("KillsPerDeathText").GetComponent<TextMeshProUGUI>().text = Mathf.Round(deaths != 0 ? kills / deaths : kills).ToString("0.00");

                if (winner == player.NickName)
                {
                    leaderboardSlot.transform.Find("WinnerImage").gameObject.SetActive(true);
                }
                else
                {
                    leaderboardSlot.transform.Find("WinnerImage").gameObject.SetActive(false);
                }
                Destroy(leaderboardSlot, 15);
            }
        }

        public void DisableLeaderboardScreen()
        {
            endScreenPanel.SetActive(false);
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
                //player.GetComponent<Movement>().SetAmmo(true, 0, 0);
                //player.GetComponent<PlayerHealth>().ReplenishHealth(200, 2);
                startedGame = false;
                StopAllCoroutines();
                playerPhotonView.RPC("StartSpawn", RpcTarget.AllBuffered, 10f);
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

        [PunRPC]
        public void DisableGameObject(string name, string parent)
        {
            if (parent != null)
            {
                GameObject.Find(parent).transform.Find(name).gameObject.SetActive(false);
            } else
            {
                GameObject.Find(name).SetActive(false);
            }

            Debug.Log("Set " + name + " to be inactive");
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

