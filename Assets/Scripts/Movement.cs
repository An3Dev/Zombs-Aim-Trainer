using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro;
using System;
using Photon.Realtime;
using UnityEditorInternal;

public class Movement : MonoBehaviour
{

    public float speed = 1;

    float xMovement, yMovement;
    public Camera mainCamera;

    public enum PlayerState { Weapon, Building, Editing};

    public PlayerState playerState = PlayerState.Weapon;

    //Shoot activeShootScript;
    //Building buildingScript;
    //Editing editingScript;
    public bool isTraining = true;

    public static Movement Instance;

    public Rigidbody2D rb;


    PlayerState previousState;

    public PhotonView photonView;

    GameObject[] wallsList;

    Inventory inventory;

    Sprite buildingSprite, editingSprite;

    [Space]

    public KeyCode handsKeybinds = KeyCode.Tab, firstSlotKeybind = KeyCode.Alpha1, secondSlotKeybind = KeyCode.Alpha2, thirdSlotKeybind = KeyCode.Alpha3,
            fourthSlotKeybind = KeyCode.Alpha4, fifthSlotKeybind = KeyCode.Alpha5, reloadKeybind = KeyCode.R;

    Item currentItem;

    [SerializeField] private SpriteRenderer gunRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        //activeShootScript = GetComponentInChildren<Shoot>();
        //buildingScript = GetComponent<Building>();
        //editingScript = GetComponent<Editing>();
        Instance = this;
        photonView = GetComponent<PhotonView>();

        mainCamera = Camera.main;

        if(!photonView.IsMine && !An3Apps.GameManager.testMode)
        {
            //Destroy(buildingScript);
            Destroy(GetComponent<Movement>());
            //Destroy(editingScript);
            //buildingScript.enabled = false;
            //editingScript.GetComponent<Editing>().enabled = false;
        }

        editWall = GameObject.Find("EditWall");
        leftEdit = GameObject.Find("LeftEdit");
        rightEdit = GameObject.Find("RightEdit");
        leftEditPress = GameObject.Find("LeftEditPress");
        rightEditPress = GameObject.Find("RightEditPress");

        leftEditPress.SetActive(false);
        rightEditPress.SetActive(false);
        editWall.SetActive(false);

        gameManagerPhotonView = GameObject.Find("GameManager").GetComponent<PhotonView>();
        inventory = GetComponentInChildren<Inventory>();
    }


    // Start is called before the first frame update
    void Start()
    {
        //photonView = GetComponent<PhotonView>();
        mainCamera = Camera.main;

        wallPreview = GameObject.Find("WallPreview");
        wallPreview.SetActive(false);
        killsText = GameObject.Find("KillsText").GetComponent<TextMeshProUGUI>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && !An3Apps.GameManager.testMode)
        {
            return;
        }

        xMovement = Input.GetAxis("Horizontal");
        yMovement = Input.GetAxis("Vertical");


        //transform.Translate((transform.position + force) * speed * Time.deltaTime);


        //transform.position += force * speed * Time.deltaTime;
        //transform.position = new Vector3(Mathf.Clamp(transform.position.x, -10, 10), Mathf.Clamp(transform.position.y, -5, 5));
        Rotate();

        if (Input.GetMouseButton(0))
        {
            if (playerState == PlayerState.Weapon)
            {
                //activeShootScript.TryShooting();
                TryShooting();
                //buildingScript.StopBuilding();
                StopBuilding();
            }
            else if (playerState == PlayerState.Building)
            {
                //buildingScript.PlaceWall();
                PlaceWall();
            }
            else if (playerState == PlayerState.Editing)
            {
                if (isEditingWall)
                {
                    // edit
                    Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
                    Vector3 direction = (mouseWorldPos - transform.root.position).normalized;

                    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, editDistance, wallMask);

                    Debug.DrawLine(transform.position, (transform.position + direction) * editDistance);

                    if (hit)
                    {
                        string colliderName = hit.collider.name;
                        if (colliderName == "LeftEdit" && lastEdited != "LeftEditPress")
                        {
                            leftEdit.SetActive(false);
                            leftEditPress.SetActive(true);
                            lastEdited = hit.collider.gameObject.name;
                        }
                        else if (colliderName == "RightEdit" && lastEdited != "RightEditPress")
                        {
                            rightEdit.SetActive(false);
                            rightEditPress.SetActive(true);
                            lastEdited = hit.collider.gameObject.name;

                        }
                        else if (colliderName == "LeftEditPress" && lastEdited != "LeftEdit")
                        {
                            leftEditPress.SetActive(false);

                            leftEdit.SetActive(true);
                            lastEdited = hit.collider.gameObject.name;

                        }
                        else if (colliderName == "RightEditPress" && lastEdited != "RightEdit")
                        {
                            rightEditPress.SetActive(false);

                            rightEdit.SetActive(true);
                            lastEdited = hit.collider.gameObject.name;

                        }
                    }
                }
            }
        }

        if (isEditingWall)
        {
            if (Input.GetMouseButtonUp(0))
            {
                lastEdited = "";
                if (editOnRelease)
                {
                    Confirm();
                }

            }
        } 
        if (isEditingWall)
        {
            Debug.Log("Editing wall");
            // right click
            if (Input.GetMouseButton(1))
            {

                lastEdited = "";
                leftEdit.SetActive(true);
                leftEditPress.SetActive(false);
                rightEdit.SetActive(true);
                rightEditPress.SetActive(false);

            }

            if (Input.GetMouseButtonUp(1))
            {
                if (resetOnRelease)
                {
                    Confirm();
                }
            }
        }   

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchStates(PlayerState.Building);
            
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            previousState = playerState;

            SwitchStates(PlayerState.Editing);
            playerState = Edit() ? PlayerState.Editing : previousState;
        }

        //// if scrollwheel is used switch to weapon
        //if (Input.GetAxis("Mouse ScrollWheel") > 0.25f || Input.GetAxis("Mouse ScrollWheel") < -0.25f || Input.GetKey(KeyCode.Alpha1))
        //{
        //    Switch(State)
        //    //buildingScript.StopBuilding();
        //    StopBuilding();
        //}

        if (isBuilding)
        {
            tempWallPosition = PositionWall();

            RaycastHit2D hit = Physics2D.Linecast(transform.position, tempWallPosition, buildsMask);

            if (hit)
            {
                //Debug.Log(hit.transform.name);
                wallPreview.SetActive(false);
            }
            else
            {
                wallPreview.SetActive(true);
            }

            //if (Physics2D.OverlapPoint(tempWallPosition, buildsMask))
            //{
            //    return;
            //}

            wallPreview.transform.position = tempWallPosition;
            wallPreview.transform.rotation = tempWallRotation;
        }

        // Keybinds for weapons

        if (Input.GetKeyDown(firstSlotKeybind)) {
            currentItem = inventory.SelectItem(0);
            SwitchtoWeapon(currentItem);
            photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem);
        }
        else if (Input.GetKeyDown(secondSlotKeybind))
        {
            currentItem = inventory.SelectItem(1);

            SwitchtoWeapon(currentItem);
        }
        else if (Input.GetKeyDown(thirdSlotKeybind))
        {
            currentItem = inventory.SelectItem(2);

            SwitchtoWeapon(currentItem);
        }
        else if (Input.GetKeyDown(fourthSlotKeybind))
        {
            currentItem = inventory.SelectItem(3);

            SwitchtoWeapon(currentItem);
        }
        else if (Input.GetKeyDown(fifthSlotKeybind))
        {
            currentItem = inventory.SelectItem(4);

            SwitchtoWeapon(currentItem);
        }
    }

    [PunRPC]
    void SwitchtoWeapon(Item currentItem)
    {
        SwitchStates(PlayerState.Weapon);

        Debug.Log(currentItem);

        // set stats.
        SetStats(currentItem);

        // Show item.
        gunRenderer.sprite = currentItem.topViewSprite;

        // reset time since last shot;
        lastTimeShot = Time.timeSinceLevelLoad - timeBetweenShots;
    }

    void SetStats(Item item)
    {
        timeBetweenShots = currentItem.timeBetweenShots;
        bulletSpeed = currentItem.bulletSpeed;
        bulletDamage = currentItem.damage;
        bulletSprite = currentItem.bulletSprite;
    }


    void SwitchStates(PlayerState stateToChangeTo)
    {
        if (stateToChangeTo == PlayerState.Building)
        {
            CancelEdit();
            gunRenderer.sprite = buildingSprite;

            StartBuilding();
        }
        else if (stateToChangeTo == PlayerState.Editing)
        {
            StopBuilding();
            gunRenderer.sprite = editingSprite;
        } else if (stateToChangeTo == PlayerState.Weapon)
        {
            StopBuilding();
            CancelEdit();
        }

        playerState = stateToChangeTo;
    }

    #region movement code


    //void StartBuilding()
    //{
    //    buildingScript.StartBuilding();
    //}

    //void CancelEdit()
    //{
    //    Editing.Instance.CancelEdit();
    //}

    public void FixedUpdate()
    {
        Vector3 force = new Vector3(xMovement, yMovement);

        if (force.sqrMagnitude > 1)
        {
            force = force.normalized;
        }
        rb.velocity = force * speed;

        if (xMovement == 0 && yMovement == 0)
        {
            rb.velocity = Vector2.zero;
            rb.Sleep();
        }
    }

    public void StoppedEditing()
    {
        playerState = previousState;
        if (previousState == PlayerState.Building)
        {
            StartBuilding();
        }
        
    }
    void Rotate()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
        Vector3 direction = (mouseWorldPos - transform.position).normalized;
        transform.up = direction;
    }

    #endregion Movement code

    #region Building code
    float gridSpacing = 2.8f;

    bool isBuilding = false;

    //Camera mainCamera;

    Vector3 tempWallPosition;
    Quaternion tempWallRotation;

    public GameObject wallPrefab;
    public GameObject wallPreview;

    public LayerMask buildsMask;


    public void StartBuilding()
    {
        isBuilding = true;

        // show transparent wall
        wallPreview.SetActive(true);
    }

    public void StopBuilding()
    {
        isBuilding = false;
        wallPreview.SetActive(false);
    }

    [PunRPC]
    public void AddWall(GameObject wall)
    {
        wallsList.Append(wall);
    }

    public void FindWallInArray(GameObject gameObject)
    {
        int index = 0;
        for (int i = 0; i < wallsList.Length; i++)
        {
            if (wallsList[i] == gameObject)
            {
                index = i;
                break;
            }
        }

        Debug.Log(wallsList[index].transform.position);
    }

    public void PlaceWall()
    {
        if (Physics2D.OverlapPoint(tempWallPosition, buildsMask))
        {
            return;
        }

        try
        {
            PhotonNetwork.Instantiate("Wall", tempWallPosition, tempWallRotation);
        }
        catch
        {
            Instantiate(wallPrefab, tempWallPosition, tempWallRotation);
            Debug.Log("test");
        }

        if (PhotonNetwork.OfflineMode)
        {
            Instantiate(wallPrefab, tempWallPosition, tempWallRotation);
        }

        //Debug.Log(tempWallPosition);
    }


    Vector2 PositionWall()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
        Vector3 direction = (mouseWorldPos - transform.position).normalized;

        Vector3 finalPosition = Vector3.zero;

        if ((transform.rotation.eulerAngles.z > 135 && transform.rotation.eulerAngles.z < 225) || (transform.rotation.eulerAngles.z > 315 || transform.rotation.eulerAngles.z < 45))
        {
            // place horizontal oriented wall on top of player;
            tempWallRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
        else
        {
            tempWallRotation = Quaternion.Euler(Vector3.zero);
        }

        Vector3 temp = transform.position + direction * 1.15f;

        Physics2D.queriesHitTriggers = false;
        RaycastHit2D hit = Physics2D.Linecast(transform.position, temp, buildsMask);
        Physics2D.queriesHitTriggers = true;


        Vector3 finalDirPoint = (hit ? new Vector3(hit.point.x, hit.point.y, 0) : temp);

        //Debug.DrawLine(transform.position, finalDirPoint, Color.grey);

        //Debug.DrawLine(transform.position, temp, Color.blue);

        finalPosition = GetGridPosition(finalDirPoint);

        //Debug.DrawLine(transform.position, finalPosition, Color.red);
        return finalPosition;
    }

    Vector2 GetGridPosition(Vector2 value)
    {
        Vector3 position = Vector2.zero;
        if (tempWallRotation.eulerAngles != Vector3.zero)
        {
            position.x += gridSpacing / 2;

            //if (value.x > transform.position.x)
            //{
            //    //Debug.Log("Right");
            //    value.x -= gridSpacing / 2;

            //}
            //else if (value.x < transform.position.x)
            //{
            //    //Debug.Log("Left");
            //    value.x += gridSpacing / 2;
            //}

            if (value.y > transform.position.y)
            {
                //Debug.Log("Up");
                position.y += gridSpacing / 2;
            }
            else if (value.y < transform.position.y)
            {
                //Debug.Log("Down");
                position.y -= gridSpacing / 2;
            }
        }
        position.x += Mathf.Round((value.x / (float)gridSpacing)) * gridSpacing;
        position.y += Mathf.Round((value.y / (float)gridSpacing)) * gridSpacing;

        if (tempWallRotation.eulerAngles != Vector3.zero)
        {
            if ((position - transform.position).sqrMagnitude > (new Vector3(position.x - gridSpacing, position.y, 0) - transform.position).sqrMagnitude)
            {
                position.x -= gridSpacing;
            }
        }

        return position;
    }

#endregion Building code

    #region Shooting code
    public Transform bulletOrigin;
    public GameObject bulletPrefab;

    float lastTimeShot;

    public float timeBetweenShots = 0.2f;
    public float reloadTime = 4;

    public int timesShot;

    float bulletSpeed = 25;
    float bulletDamage = 25;

    Sprite bulletSprite;

    int kills = 0;
    public TextMeshProUGUI killsText;

    public void IncreaseKills(int amount)
    {
        kills += amount;
        killsText.text = kills.ToString();
    }

    public void TryShooting()
    {
        if (Time.timeSinceLevelLoad - lastTimeShot >= timeBetweenShots)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
            Vector3 direction = (mouseWorldPos - transform.root.position).normalized;

            SpawnBullet(bulletOrigin.position, direction);
            try
            {
                photonView.RPC("SpawnBullet", RpcTarget.AllBuffered, bulletOrigin.position, direction);
            }
            catch
            {
                Debug.Log("Error spawning bullet online");
            }

            lastTimeShot = Time.timeSinceLevelLoad;
            timesShot++;

        }
    }

    [PunRPC]
    public void SpawnBullet(Vector3 location, Vector3 direction)
    {
        //Debug.Log("Info: " + info);
        GameObject bullet = Instantiate(bulletPrefab, location, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetStats(bulletSpeed, bulletDamage);
        bullet.GetComponent<SpriteRenderer>().sprite = bulletSprite;
        bullet.transform.up = direction;
        bullet.SendMessage("AssignParent", photonView.ViewID);
    }
    #endregion Shooting code

    #region Editing code

    public GameObject editWall;
    public GameObject leftEdit, rightEdit, leftEditPress, rightEditPress;

    bool isEditingWall = false;

    public LayerMask wallMask;

    GameObject disabledWall;

    string lastEdited;

    GameObject leftWall, rightWall, regularWall;
    PhotonView leftWallPhotonView, rightWallPhotonView, regularWallPhotonView;

    public bool editOnRelease;
    public bool resetOnRelease = true;

    public float editDistance;

    PhotonView gameManagerPhotonView;

    public void CancelEdit()
    {
        EnableEditingWall(false);
        disabledWall = null;
        isEditingWall = false;
        //ResetEditWall();     
    }

    public bool Edit()
    {
        if (isEditingWall)
        {
            Debug.Log("Confirm");
            isEditingWall = false;

            // confirm
            Confirm();

            return false;
        }
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
        Vector3 direction = (mouseWorldPos - transform.root.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 5, wallMask);

        if (hit)
        {
            if (hit.collider.CompareTag("Wall"))
            {

                isEditingWall = true;
                disabledWall = hit.collider.transform.root.gameObject;

                regularWall = disabledWall.transform.GetChild(0).gameObject;
                leftWall = disabledWall.transform.GetChild(1).gameObject;
                rightWall = disabledWall.transform.GetChild(2).gameObject;

                try
                {
                    regularWallPhotonView = regularWall.GetComponent<PhotonView>();
                    leftWallPhotonView = leftWall.GetComponent<PhotonView>();
                    rightWallPhotonView = rightWall.GetComponent<PhotonView>();
                }
                catch
                {
                    Debug.Log("No photonView");
                }

                if (leftWall.activeInHierarchy)
                {
                    rightEditPress.SetActive(true);
                    rightEdit.SetActive(false);
                }

                if (rightWall.activeInHierarchy)
                {
                    leftEditPress.SetActive(true);
                    leftEdit.SetActive(false);
                }

                PositionEditWall(disabledWall.transform);

                return true;
            }
        }
        return false;
    }

    void Confirm()
    {
        // enables sprite renderer of active wall.
        for (int i = 0; i < disabledWall.transform.childCount; i++)
        {
            if (disabledWall.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                disabledWall.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        // if both are pressed, don't edit and show regular wall or if both edit squares are not selected
        if ((leftEditPress.activeInHierarchy && rightEditPress.activeInHierarchy) || (leftEdit.activeInHierarchy && rightEdit.activeInHierarchy))
        {
            // showRegularWall
            regularWall.SetActive(true);
            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, true, regularWallPhotonView.ViewID);

            leftWall.SetActive(false);
            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, leftWallPhotonView.ViewID);

            rightWall.SetActive(false);
            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, rightWallPhotonView.ViewID);

        }

        if (leftEditPress.activeInHierarchy)
        {
            rightWall.SetActive(true);
            leftWall.SetActive(false);
            regularWall.SetActive(false);

            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, regularWallPhotonView.ViewID);
            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, leftWallPhotonView.ViewID);
            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, true, rightWallPhotonView.ViewID);


        }
        else if (rightEditPress.activeInHierarchy)
        {
            leftWall.SetActive(true);
            rightWall.SetActive(false);
            regularWall.SetActive(false);

            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, true, leftWallPhotonView.ViewID);
            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, rightWallPhotonView.ViewID);
            gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, regularWallPhotonView.ViewID);

        }

        ResetEditWall();
        editWall.SetActive(false);
        disabledWall.SetActive(true);
        StoppedEditing();

        isEditingWall = false;
    }

    void ResetEditWall()
    {
        leftEdit.SetActive(true);

        rightEdit.SetActive(true);

        leftEditPress.SetActive(false);
        rightEditPress.SetActive(false);
    }

    void PositionEditWall(Transform wall)
    {
        editWall.transform.position = wall.position;
        editWall.transform.rotation = wall.rotation;

        EnableEditingWall(true);
    }

    void EnableEditingWall(bool enable)
    {
        if (disabledWall == null)
        {
            return;
        }
        for (int i = 0; i < disabledWall.transform.childCount; i++)
        {
            if (disabledWall.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                disabledWall.transform.
                    GetChild(i).GetComponent<SpriteRenderer>().enabled = !enable;
            }
        }
        //disabledWall.gameObject.SetActive(!enable);

        editWall.SetActive(enable);
    }

    #endregion Editing code
}
