using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro;
using System;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;

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

    PlayerState lastState;

    [Space]

    public KeyCode handsKeybinds = KeyCode.Tab, firstSlotKeybind = KeyCode.Alpha1, secondSlotKeybind = KeyCode.Alpha2, thirdSlotKeybind = KeyCode.Alpha3,
            fourthSlotKeybind = KeyCode.Alpha4, fifthSlotKeybind = KeyCode.Alpha5, reloadKeybind = KeyCode.R;

    Item currentItem;

    [SerializeField] private SpriteRenderer gunRenderer;

    [SerializeField] Texture2D cursorTexture;
    Vector2 cursorHotspot;

    // Start is called before the first frame update
    void Awake()
    {
        //activeShootScript = GetComponentInChildren<Shoot>();
        //buildingScript = GetComponent<Building>();
        //editingScript = GetComponent<Editing>();
        Instance = this;
        photonView = GetComponent<PhotonView>();

        if (!photonView.IsMine && !An3Apps.GameManager.testMode)
        {
            return;
        }

        Debug.Log(photonView.ViewID + " Is Mine in awake");

        mainCamera = Camera.main;

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

        ammoUI = GameObject.Find("AmmoUI");
        currentBulletsInMagText = GetChildByName("CurrentAmmoInMagText", ammoUI.transform).GetComponent<TextMeshProUGUI>();
        totalAmmoText = GetChildByName("TotalAmmoText", ammoUI.transform).GetComponent<TextMeshProUGUI>();
        reloadCircle = GetChildByName("ReloadCircle", ammoUI.transform).GetComponent<Image>();

        cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);

        totalAmmoList = new List<int>();
        currentBulletsInMagList = new List<int>();
        //SetAmmo(true, 0, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        wallPreview = GameObject.Find("WallPreview");
        wallPreview.SetActive(false);

        mainCamera = Camera.main;

        killsText = GameObject.Find("KillsText").GetComponent<TextMeshProUGUI>();


        totalAmmoList = new List<int>();
        currentBulletsInMagList = new List<int>();

        currentItem = inventory.SelectItem(0);
        Debug.Log(inventory.itemsInInventory.Count);

        SwitchToWeapon(currentItem.name);
        photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem.name);

        if (!photonView.IsMine)
        {
            return;
        }
    }

    Transform GetChildByName(string name, Transform parent)
    {
        for(int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
            {
                return parent.GetChild(i);
            }
        }
        return null;
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
                StopBuilding();
                TryShooting();
                             
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
            Debug.Log("Shift");
            if (playerState != PlayerState.Editing)
            {
                lastState = playerState;
            }
            SwitchStates(PlayerState.Editing);
            playerState = Edit() ? PlayerState.Editing : lastState;

            if (playerState == PlayerState.Editing)
            {
                
            } else if (playerState == PlayerState.Weapon)
            {
                Debug.Log("Weapon");
                SwitchToWeapon(currentItem.name);
            }
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

            wallPreview.transform.position = tempWallPosition;
            wallPreview.transform.rotation = tempWallRotation;
        }

        // Keybinds for weapons

        if (Input.GetKeyDown(firstSlotKeybind)) {
            currentItem = inventory.SelectItem(0);

            if (PhotonNetwork.OfflineMode)
            {
                SwitchToWeapon(currentItem.name);
            }
            else
            {
                photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem.name);
            }
        }
        else if (Input.GetKeyDown(secondSlotKeybind))
        {
            currentItem = inventory.SelectItem(1);

            if (PhotonNetwork.OfflineMode)
            {
                SwitchToWeapon(currentItem.name);
            }
            else
            {
                photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem.name);
            }

        }
        else if (Input.GetKeyDown(thirdSlotKeybind))
        {
            currentItem = inventory.SelectItem(2);

            if (PhotonNetwork.OfflineMode)
            {
                SwitchToWeapon(currentItem.name);
            }
            else
            {
                photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem.name);
            }

        }
        else if (Input.GetKeyDown(fourthSlotKeybind))
        {
            currentItem = inventory.SelectItem(3);

            if (PhotonNetwork.OfflineMode)
            {
                SwitchToWeapon(currentItem.name);
            }
            else
            {
                photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem.name);
            }

        }
        else if (Input.GetKeyDown(fifthSlotKeybind))
        {
            currentItem = inventory.SelectItem(4);

            if (PhotonNetwork.OfflineMode)
            {
                SwitchToWeapon(currentItem.name);
            } else
            {
                photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem.name);
            }

        }
        else if (Input.GetKeyDown(reloadKeybind))
        {
            Reload();
        }
        
    }

    void SwitchStates(PlayerState stateToChangeTo)
    {
        playerState = stateToChangeTo;

        if (stateToChangeTo == PlayerState.Building)
        {
            gunRenderer.sprite = buildingSprite;
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;
            lastState = PlayerState.Building;

        }
        else if (stateToChangeTo == PlayerState.Editing)
        {
            gunRenderer.sprite = editingSprite;
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;

        }

        if (!photonView.IsMine)
        {
            return;
        }
        if (stateToChangeTo == PlayerState.Building)
        {
            gunRenderer.sprite = buildingSprite;
            CancelEdit();        
            StartBuilding();
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;
            lastState = PlayerState.Building;

        }
        else if (stateToChangeTo == PlayerState.Editing)
        {
            gunRenderer.sprite = editingSprite;
            StopBuilding();
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;
            
        } else if (stateToChangeTo == PlayerState.Weapon)
        {
            StopBuilding();
            CancelEdit();
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = true;
            lastState = PlayerState.Weapon;

        }
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

        Vector3 gunDirection = (mouseWorldPos - bulletOrigin.root.position).normalized;
        gunRenderer.transform.right = gunDirection;
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

        if (!PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.Instantiate("Wall", tempWallPosition, tempWallRotation);
        } else
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

    List<int> totalAmmoList;
    List<int> currentBulletsInMagList;

    float startingAmmoMultiplier = 3;

    int totalBulletsAvailable;
    int currentBulletsInMag;

    GameObject ammoUI;
    TextMeshProUGUI totalAmmoText, currentBulletsInMagText;
    Image reloadCircle;

    int amountOfBulletsShot = 1;

    bool isReloading = false;

    public void IncreaseKills(int amount)
    {
        kills += amount;
        killsText.text = kills.ToString();
        // Add score
        photonView.Owner.AddScore(1);
    }

    // Sets the ammo count of all the weapons. Also swaps ammo when weapons are moved in the inventory
    public void SetAmmo(bool setToMax, int from, int to)
    {
        // if want to set the ammo to its default staring ammo.
        if (setToMax)
        {
            totalAmmoList.Clear();
            currentBulletsInMagList.Clear();
            for (int i = 0; i < inventory.itemsInInventory.Count; i++)
            {
                totalAmmoList.Insert(i, (inventory.itemsInInventory[i].startingAmmo));
                currentBulletsInMagList.Insert(i, inventory.itemsInInventory[i].magazineSize);
            }
            SwitchToWeapon(currentItem.name);
        }
        else
        {
            // swaps data.
            // total ammo data
            int toAmount = totalAmmoList[to];
            int fromAmount = totalAmmoList[from];
            totalAmmoList.RemoveAt(to);
            totalAmmoList.RemoveAt(from);

            totalAmmoList.Insert(from, toAmount);
            totalAmmoList.Insert(to, fromAmount);

            // current bullets data
            toAmount = currentBulletsInMagList[to];
            fromAmount = currentBulletsInMagList[from];
            currentBulletsInMagList.RemoveAt(to);
            currentBulletsInMagList.RemoveAt(from);

            currentBulletsInMagList.Insert(from, toAmount);
            currentBulletsInMagList.Insert(to, fromAmount);
        }

        //UpdateAmmoList();
    }

    [PunRPC]
    void SwitchToWeapon(string currentItem)
    {
        Item thisItem = Resources.Load("Guns/" + currentItem) as Item;

        // Show item.
        gunRenderer.sprite = thisItem.topViewSprite;


        if (!photonView.IsMine)
        {
            return;
        }

        SwitchStates(PlayerState.Weapon);
        SetStats(thisItem);

        StopCoroutine("StartReload");
        StopCoroutine("ReloadAnimation");
        reloadCircle.gameObject.SetActive(false);

       

        // reset time since last shot;
        lastTimeShot = Time.timeSinceLevelLoad - timeBetweenShots;
    }

    void SetStats(Item item)
    {
        timeBetweenShots = item.timeBetweenShots;
        bulletSpeed = item.bulletSpeed;
        bulletDamage = item.damage;
        bulletSprite = item.bulletSprite;
        reloadTime = item.reloadTime;
        amountOfBulletsShot = item.bulletsShotAtOnce;

        // if not in offline mode, check if photon view is mine. If in offline mode, if statement is true.
        if (!PhotonNetwork.OfflineMode ? photonView.IsMine : 1==1)
        {
            totalBulletsAvailable = totalAmmoList[inventory.GetItemIndex(item)];
            currentBulletsInMag = currentBulletsInMagList[inventory.GetItemIndex(item)];
        }

        UpdateAmmoList();
    }

    void Reload()
    {
        if (currentBulletsInMag >= currentItem.magazineSize)
        {
            Debug.Log("No ammo");
            return;
        }
        StartCoroutine("StartReload", reloadTime);
    }

    IEnumerator StartReload(float time)
    {
        StartCoroutine("ReloadAnimation", time);
        yield return new WaitForSeconds(time);
        
        currentBulletsInMag = totalBulletsAvailable >= currentItem.magazineSize ? currentItem.magazineSize : totalBulletsAvailable;
        UpdateAmmoList();
        Debug.Log("Reloaded in " + time + " seconds");
        StopCoroutine("StartReload");
    }

    IEnumerator ReloadAnimation(float time)
    {
        float duration = time; 
        float normalizedTime = 0;
        reloadCircle.gameObject.SetActive(true);
        while (normalizedTime <= 1f)
        {
            reloadCircle.fillAmount = normalizedTime;
            reloadCircle.transform.position = Input.mousePosition;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
        reloadCircle.gameObject.SetActive(false);

    }

    public void TryShooting()
    {
        if (Time.timeSinceLevelLoad - lastTimeShot > timeBetweenShots)
        {
            if (currentBulletsInMag <= 0)
            {
                Debug.Log("No bullets in mag");
                return;
            }

            StopCoroutine("StartReload");
            StopCoroutine("ReloadAnimation");
            reloadCircle.gameObject.SetActive(false);

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
            Vector3 direction = (mouseWorldPos - bulletOrigin.position).normalized;

            float maxAngle = 70;


            if (PhotonNetwork.OfflineMode)
            {
                for(int i = 0; i < amountOfBulletsShot; i++)
                {
                    //Vector3 angle = 
                    SpawnBullet(bulletOrigin.position, direction, bulletSpeed, bulletDamage, currentItem.timeBeforeDestroy);
                }
            } else
            {
                try
                {
                    photonView.RPC("SpawnBullet", RpcTarget.AllBufferedViaServer, bulletOrigin.position, direction, bulletSpeed, bulletDamage, currentItem.timeBeforeDestroy);
                }
                catch
                {
                    Debug.Log("Spawning bullet error");
                }
            }        

            lastTimeShot = Time.timeSinceLevelLoad;
            timesShot++;

            totalBulletsAvailable--;
            currentBulletsInMag--;
            UpdateAmmoList();
        }
    }

    void UpdateAmmoList()
    {
        totalAmmoList[inventory.GetItemIndex(currentItem)] = totalBulletsAvailable;
        currentBulletsInMagList[inventory.GetItemIndex(currentItem)] = currentBulletsInMag;

        currentBulletsInMagText.text = currentBulletsInMag.ToString();

        int totalAmmo = currentItem.magazineSize - currentBulletsInMag + totalBulletsAvailable - currentItem.magazineSize;
        totalAmmoText.text = "/" + totalAmmo;
    }

    [PunRPC]
    public void SpawnBullet(Vector3 location, Vector3 direction, float speedOfBullet, float damageOfBullet, float timeBeforeDestroy)
    {
        GameObject bullet = Instantiate(bulletPrefab, location, Quaternion.identity);
        //Debug.Log("Bullet script: " + bullet.GetComponent<Bullet>());
        bullet.GetComponent<Bullet>().SetStats(speedOfBullet, damageOfBullet, timeBeforeDestroy);
        bullet.GetComponent<SpriteRenderer>().sprite = bulletSprite;
        bullet.transform.up = direction;

        if (!PhotonNetwork.OfflineMode)
        {
            //bullet.SendMessage("AssignParent", photonView.ViewID);
 
            bullet.SendMessage("AssignParent", photonView.ViewID);

        }
        else
        {
            bullet.SendMessage("AssignParentTransform", transform.root);
        }
        //Debug.Log("Spawned bullet");
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
        Debug.Log("Edit");
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

            if (!PhotonNetwork.OfflineMode)
            {
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, true, regularWallPhotonView.ViewID);
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, leftWallPhotonView.ViewID);
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, rightWallPhotonView.ViewID);

            }
            // showRegularWall
            regularWall.SetActive(true);

            leftWall.SetActive(false);

            rightWall.SetActive(false);

        }

        if (leftEditPress.activeInHierarchy)
        {
            rightWall.SetActive(true);
            leftWall.SetActive(false);
            regularWall.SetActive(false);

            if (!PhotonNetwork.OfflineMode)
            {
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, regularWallPhotonView.ViewID);
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, leftWallPhotonView.ViewID);
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, true, rightWallPhotonView.ViewID);
            }

        }
        else if (rightEditPress.activeInHierarchy)
        {
            leftWall.SetActive(true);
            rightWall.SetActive(false);
            regularWall.SetActive(false);

            if (!PhotonNetwork.OfflineMode)
            {
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, true, leftWallPhotonView.ViewID);
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, rightWallPhotonView.ViewID);
                gameManagerPhotonView.RPC("EnableGameObject", RpcTarget.AllBufferedViaServer, false, regularWallPhotonView.ViewID);
            }
        }

        ResetEditWall();
        editWall.SetActive(false);
        disabledWall.SetActive(true);
        StoppedEditing();

        Debug.Log(lastState);
        if (lastState == PlayerState.Weapon)
        {
            SwitchToWeapon(currentItem.name);
            
        } else if (lastState == PlayerState.Building)
        {
            SwitchStates(PlayerState.Building);
            StartBuilding();
        }

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
