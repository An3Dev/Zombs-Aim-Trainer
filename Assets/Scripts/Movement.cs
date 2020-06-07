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
using UnityEngine.EventSystems;
using An3Apps;

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

    [SerializeField]Sprite buildingSprite, editingSprite;

    PlayerState lastState;

    [Space]

    public KeyCode handsKeybinds = KeyCode.Tab, firstSlotKeybind = KeyCode.Alpha1, secondSlotKeybind = KeyCode.Alpha2, thirdSlotKeybind = KeyCode.Alpha3,
            fourthSlotKeybind = KeyCode.Alpha4, fifthSlotKeybind = KeyCode.Alpha5, reloadKeybind = KeyCode.R;

    Item currentItem;

    [SerializeField] private SpriteRenderer gunRenderer;

    [SerializeField] Texture2D cursorTexture;
    Vector2 cursorHotspot;

    TextMeshProUGUI reloadCircleText;

    List<float> materialCountList;
    int maxMaterials = 10;
    int currentMaterial = 0;

    PlayerHealth playerHealth;

    [SerializeField] GameObject woodWallPrefab, brickWallPrefab, metalWallPrefab;

    [SerializeField] TextMeshProUGUI woodText, brickText, metalText;
    // Start is called before the first frame update
    void Awake()
    {
        //activeShootScript = GetComponentInChildren<Shoot>();
        //buildingScript = GetComponent<Building>();
        //editingScript = GetComponent<Editing>();
        Instance = this;
        photonView = GetComponent<PhotonView>();

        GameObject group = GameObject.Find("MaterialsGroup");

        woodText = group.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        brickText = group.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        metalText = group.transform.GetChild(2).GetComponent<TextMeshProUGUI>();



        materialCountList = new List<float>();
        if (!PhotonNetwork.OfflineMode && !photonView.IsMine)
        {
            return;
        }
        gameManagerPhotonView = GameObject.Find("GameManager").GetComponent<PhotonView>();
        inventory = GetComponentInChildren<Inventory>();

        mainCamera = Camera.main;

        editWall = GameObject.Find("EditWall");
        leftEdit = GameObject.Find("LeftEdit");
        rightEdit = GameObject.Find("RightEdit");
        leftEditPress = GameObject.Find("LeftEditPress");
        rightEditPress = GameObject.Find("RightEditPress");

        leftEditPress.SetActive(false);
        rightEditPress.SetActive(false);
        editWall.SetActive(false);



        ammoUI = GameObject.Find("AmmoUI");
        currentBulletsInMagText = GetChildByName("CurrentAmmoInMagText", ammoUI.transform).GetComponent<TextMeshProUGUI>();
        totalAmmoText = GetChildByName("TotalAmmoText", ammoUI.transform).GetComponent<TextMeshProUGUI>();
        reloadCircle = GetChildByName("ReloadCircle", ammoUI.transform).GetComponent<Image>();

        reloadCircleText = reloadCircle.GetComponentInChildren<TextMeshProUGUI>();

        cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);

        totalAmmoList = new List<int>();
        currentBulletsInMagList = new List<int>();

        playerHealth = GetComponent<PlayerHealth>();

        //SetAmmo(true, 0, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        wallPreview = GameObject.Find("WallPreview");
        wallPreview.SetActive(false);

        mainCamera = Camera.main;

        killsText = GameObject.Find("KillsText").GetComponent<TextMeshProUGUI>();

        currentItem = inventory.SelectItem(0);

        if (!PhotonNetwork.OfflineMode)
        {
            photonView.RPC("SwitchToWeapon", RpcTarget.AllBuffered, currentItem.name);
        } else
        {
            SwitchToWeapon(currentItem.name);
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
        if (!PhotonNetwork.OfflineMode && !photonView.IsMine)
        {
            return;
        }

        xMovement = Input.GetAxis("Horizontal");
        yMovement = Input.GetAxis("Vertical");

        //transform.Translate((transform.position + force) * speed * Time.deltaTime);s

        //transform.position += force * speed * Time.deltaTime;
        //transform.position = new Vector3(Mathf.Clamp(transform.position.x, -10, 10), Mathf.Clamp(transform.position.y, -5, 5));
        Rotate();

        if (Input.GetMouseButton(0))
        {
            //foreach(RaycastResult result in GetEventSystemRaycastResults())
            //{
                
            //}

            if (playerState == PlayerState.Weapon) 
            {
                if (playerState == PlayerState.Building)
                {
                    StopBuilding();
                }
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

        // right click
        if (Input.GetMouseButtonDown(1))
        {
            // Reset the edit
            if (isEditingWall)
            {
                lastEdited = "";
                leftEdit.SetActive(true);
                leftEditPress.SetActive(false);
                rightEdit.SetActive(true);
                rightEditPress.SetActive(false);
            }
            else if (playerState == PlayerState.Building) // if in build mode, cycle through the materials
            {
                SwitchMaterials(-1);
            }

        }

        if (Input.GetMouseButtonUp(1))
        {
            if (isEditingWall)
            {
                if (resetOnRelease)
                {
                    Confirm();
                }
            }         
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isEditingWall)
            {
                lastEdited = "";
                if (editOnRelease)
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

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for(int i = 0; i < wallsList.Length; i++)
                {
                    PhotonNetwork.Destroy(wallsList[i].gameObject);
                }
            }
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

    ///Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    ///Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }
    ///Gets all event systen raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    }

    void SwitchStates(PlayerState stateToChangeTo)
    {
        playerState = stateToChangeTo;

        if (stateToChangeTo == PlayerState.Building)
        {
            gunRenderer.sprite = buildingSprite;
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;
            lastState = PlayerState.Building;

            if (isEditingWall)
            {
                CancelEdit();
            }
        }
        else if (stateToChangeTo == PlayerState.Editing)
        {
            gunRenderer.sprite = editingSprite;
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;
            //lastState = PlayerState.Editing;
        }

        if (!photonView.IsMine)
        {
            return;
        }
        if (stateToChangeTo == PlayerState.Building)
        {
            gunRenderer.sprite = buildingSprite;

            if(isEditingWall)
            {
                CancelEdit();
            }

            StartBuilding();
            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;
            lastState = PlayerState.Building;
        }
        else if (stateToChangeTo == PlayerState.Editing)
        {
            gunRenderer.sprite = editingSprite;

            if (lastState == PlayerState.Editing)
            {
                StopBuilding();
            }

            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = false;
            //lastState = PlayerState.Editing;
            
        } else if (stateToChangeTo == PlayerState.Weapon)
        {
            if (lastState == PlayerState.Building)
            {
                StopBuilding();
            }
            else if (isEditingWall)
            {
                CancelEdit();
            }

            gunRenderer.transform.GetComponent<BoxCollider2D>().enabled = true;
            lastState = PlayerState.Weapon;

        }
    }

    #region movement code

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

        //Vector3 gunDirection = (mouseWorldPos - bulletOrigin.position).normalized;
        //gunRenderer.transform.right = gunDirection;
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

    GameObject GetWallPrefab(int indexOfMaterial)
    {
        GameObject wall;

        switch (indexOfMaterial)
        {
            case 0:
                wall = woodWallPrefab;
                break;
            case 1:
                wall = brickWallPrefab;
                break;
            case 2:
                wall = metalWallPrefab;
                break;
            default:
                wall = woodWallPrefab;
                break;
        }
        return wall;
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

    void SwitchMaterials(int index)
    {
        if (index > -1)
        {
            currentMaterial = index;      
        } else
        {
            // cycle through materials;
            if (currentMaterial == materialCountList.Count - 1)
            {
                currentMaterial = 0;
            }
            else
            {
                currentMaterial++;
            }
        }

        GameObject wall = GetWallPrefab(currentMaterial);

        // renderer of wall preview
        SpriteRenderer wallPreviewRenderer = wallPreview.GetComponent<SpriteRenderer>();
        SpriteRenderer wallRenderer = wall.GetComponentInChildren<SpriteRenderer>();
        wallPreviewRenderer.sprite = wallRenderer.sprite;
        wallPreviewRenderer.color = new Color(wallRenderer.color.r, wallRenderer.color.g, wallRenderer.color.b, wallPreviewRenderer.color.a);
    }

    public void PlaceWall()
    {
        if (Physics2D.OverlapPoint(tempWallPosition, buildsMask))
        {
            return;
        }

        // if the slot has mats switch index to that material
        if (materialCountList[currentMaterial] < 1)
        {
            for(int i = 0; i < materialCountList.Count; i++)
            {
                if(materialCountList[i] > 0)
                {
                    SwitchMaterials(i);
                }
            }
        }

        // if has materials
        if (materialCountList[currentMaterial] > 0)
        {
            GameObject wall = GetWallPrefab(currentMaterial);
             
            if (!PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Instantiate(wall.name, tempWallPosition, tempWallRotation);
            }
            else
            {
                Instantiate(wall, tempWallPosition, tempWallRotation);
            }

            materialCountList[currentMaterial] -= 1;

            woodText.text = "Wood: " + materialCountList[0];
            brickText.text = "Brick: " + materialCountList[1];
            metalText.text = "Metal: " + materialCountList[2];

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

    public List<int> totalAmmoList;
    public List<int> currentBulletsInMagList;

    int totalBulletsAvailable;
    int currentBulletsInMag;

    GameObject ammoUI;
    TextMeshProUGUI totalAmmoText, currentBulletsInMagText;
    Image reloadCircle;

    int amountOfBulletsShot = 1;

    bool isReloading = false;

    [SerializeField] float healthIncreasePerKill = 100;

    public void IncreaseKills(int amount)
    {
        kills += amount;
        killsText.text = kills.ToString();
        // Add score
        photonView.Owner.AddScore(1);

        // give health and materials
        playerHealth.ReplenishHealth(healthIncreasePerKill, 2);

        for(int i = 0; i < materialCountList.Count; i++)
        {
            if (materialCountList[i] + 5 > maxMaterials)
            {
                materialCountList[i] = maxMaterials;

            } else
            {
                materialCountList[i] += 5;
            }
        }

        SetAmmo(true, 0, 0);
        SetMaterialUI();
    }

    // Sets the ammo count of all the weapons. Also swaps ammo when weapons are moved in the inventory
    public void SetAmmo(bool setToMax, int from, int to)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        // if want to set the ammo to its default staring ammo.
        if (setToMax)
        {
            if (totalAmmoList == null)
            {
                totalAmmoList = new List<int>();
            }
            if (currentBulletsInMagList == null)
            {
                currentBulletsInMagList = new List<int>();
            }

            totalAmmoList.Clear();
            currentBulletsInMagList.Clear();
            for (int i = 0; i < inventory.itemsInInventory.Count; i++)
            {
                totalAmmoList.Insert(i, (inventory.itemsInInventory[i].startingAmmo));
                currentBulletsInMagList.Insert(i, inventory.itemsInInventory[i].magazineSize);
            }
            if (currentItem == null)
            {
                currentItem = inventory.SelectItem(0);
            }

            SwitchToWeapon(currentItem.name);

            materialCountList.Clear();
            for(int i = 0; i < 3; i++)
            {
                materialCountList.Add(maxMaterials);
            }
            SetMaterialUI();
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

    void SetMaterialUI()
    {
        woodText.text = "Wood: " + materialCountList[0];
        brickText.text = "Brick: " + materialCountList[1];
        metalText.text = "Metal: " + materialCountList[2];
    }

    [PunRPC]
    void SwitchToWeapon(string currentItem)
    {
        Item thisItem = Resources.Load("Guns/" + currentItem) as Item;

        // Show item.
        gunRenderer.sprite = thisItem.topViewSprite;


        if (!PhotonNetwork.OfflineMode && !photonView.IsMine)
        {
            return;
        }

        SwitchStates(PlayerState.Weapon);
        SetStats(thisItem);

        StopCoroutine("StartReload");
        StopCoroutine("ReloadAnimation");
        reloadCircle.gameObject.SetActive(false);      

        // reset time since last shot;
        //lastTimeShot = Time.timeSinceLevelLoad - timeBetweenShots;
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
            Debug.Log("Full ammo");
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
            reloadCircle.transform.position = mainCamera.WorldToScreenPoint(rb.position + Vector2.down * 1.5f);
            reloadCircleText.text = (duration - (normalizedTime * duration)).ToString("0.0");
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
        reloadCircle.gameObject.SetActive(false);

    }

    public void TryShooting()
    {
        if (currentItem.isHealingItem)
        {
            Debug.Log("Healing item");
            playerHealth.ReplenishHealth(25, 2);
        }
        if (Time.timeSinceLevelLoad - lastTimeShot > timeBetweenShots)
        {
            if (currentBulletsInMag <= 0 && totalBulletsAvailable > 0)
            {
                Debug.Log("No bullets in mag");
                Reload();
                return;
            } else if (totalBulletsAvailable <= 0)
            {
                Debug.Log("No ammo");
                return;
            }

            StopCoroutine("StartReload");
            StopCoroutine("ReloadAnimation");
            reloadCircle.gameObject.SetActive(false);

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
            Vector3 direction = (mouseWorldPos - transform.root.position).normalized;

            float maxAngle = 0;

            if (amountOfBulletsShot > 1)
            {
                maxAngle = 30;
            }

            if (PhotonNetwork.OfflineMode)
            {
                float angle = Angle(direction);
                for (int i = 0; i < amountOfBulletsShot; i++)
                {
                    float thisAngle = angle + (maxAngle / 2) - (maxAngle / amountOfBulletsShot * i);
                    Vector3 newDirection = Quaternion.AngleAxis(thisAngle, Vector3.forward) * Vector3.right;

                    SpawnBullet(bulletOrigin.position, newDirection, bulletSpeed, bulletDamage, currentItem.timeBeforeDestroy);
                }
            } else
            {
                float angle = Angle(direction);
                for (int i = 0; i < amountOfBulletsShot; i++)
                {
                    float thisAngle = angle + (maxAngle / 2) - (maxAngle / amountOfBulletsShot * i);
                    Vector3 newDirection = Quaternion.AngleAxis(thisAngle, Vector3.forward) * Vector3.right;
                    SpawnBullet(bulletOrigin.position, newDirection, bulletSpeed, bulletDamage, currentItem.timeBeforeDestroy);
                    photonView.RPC("SpawnBullet", RpcTarget.Others, bulletOrigin.position, newDirection, bulletSpeed, bulletDamage, currentItem.timeBeforeDestroy);
                }
            }        

            lastTimeShot = Time.timeSinceLevelLoad;
            timesShot++;

            totalBulletsAvailable--;
            currentBulletsInMag--;

            if (currentBulletsInMag < 1 && totalBulletsAvailable > 1)   
            {
                // if auto reload is on
                Reload();
            }
            UpdateAmmoList();
            inventory.UpdateAmmo(inventory.GetItemIndex(currentItem), totalBulletsAvailable);
        }
    }

    private float Angle(Vector3 v)
    {
        // make sure the vector is normalized.
        v.Normalize();
        // get the basic angle:
        var ang = Mathf.Asin(v.y) * Mathf.Rad2Deg;

        // Since negative x values in vector3 give negative angles
        // fix the angle for 2nd and 3rd quadrants:
        if (v.x < 0)
        {
            ang = 180 - ang;
        }
        else // fix the angle for 4th quadrant:
        if (v.y < 0)
        {
            ang = 360 + ang;
        }
        return ang;
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
        if (isEditingWall)
        {
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
