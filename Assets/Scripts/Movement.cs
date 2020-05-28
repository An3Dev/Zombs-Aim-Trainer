using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Movement : MonoBehaviour
{

    public float speed = 1;

    float xMovement, yMovement;
    public Camera mainCamera;

    public enum PlayerState { Weapon, Building, Editing};

    public PlayerState playerState = PlayerState.Weapon;

    Shoot activeShootScript;
    Building buildingScript;
    Editing editingScript;
    public bool isTraining = true;

    public static Movement Instance;

    public Rigidbody2D rb;


    PlayerState previousState;

    public PhotonView photonView;

    GameObject[] wallsList;

    [Space]

    public KeyCode handsKeybinds = KeyCode.Tab, firstSlotKeybind = KeyCode.Alpha1, secondSlotKeybind = KeyCode.Alpha2, thirdSlotKeybind = KeyCode.Alpha3,
            fourthSlotKeybind = KeyCode.Alpha4, fifthSlotKeybind = KeyCode.Alpha5, reloadKeybind = KeyCode.R;

    bool enabledObject = false;

    GameObject[] itemsInInventory;
    GameObject selectedItemInInventory;

    // Start is called before the first frame update
    void Awake()
    {
        activeShootScript = GetComponentInChildren<Shoot>();
        buildingScript = GetComponent<Building>();
        editingScript = GetComponent<Editing>();
        Instance = this;
        photonView = GetComponent<PhotonView>();

        mainCamera = Camera.main;

        if(!photonView.IsMine && !An3Apps.GameManager.testMode)
        {
            Destroy(buildingScript);
            Destroy(GetComponent<Movement>());
            Destroy(editingScript);
            //buildingScript.enabled = false;
            //editingScript.GetComponent<Editing>().enabled = false;
        }
    }

    void AddItemToInventory(GameObject item)
    {
        itemsInInventory.Append<GameObject>(item);
    }

    [PunRPC]
    public void AddWall(GameObject wall)
    {
        wallsList.Append(wall);
    }

    public void FindWallInArray(GameObject gameObject)
    {
        int index = 0;
        for(int i = 0; i < wallsList.Length; i++)
        {
            if (wallsList[i] == gameObject)
            {
                index = i;
                break;
            }
        }

        Debug.Log(wallsList[index].transform.position);
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
            if(playerState == PlayerState.Weapon)
            {
                activeShootScript.TryShooting();
                buildingScript.StopBuilding();
            } else if (playerState == PlayerState.Building)
            {
                buildingScript.PlaceWall();
            }
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            playerState = PlayerState.Building;
            StartBuilding();
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            previousState = playerState;

            playerState = PlayerState.Editing;
            playerState = editingScript.Edit() ? PlayerState.Editing : previousState;

            if (playerState == PlayerState.Editing)
            {
                buildingScript.StopBuilding();
                
            } else if (playerState == PlayerState.Building)
            {
                StartBuilding();
            }
        }

        // if scrollwheel is used switch to weapon
        if (Input.GetAxis("Mouse ScrollWheel") > 0.25f || Input.GetAxis("Mouse ScrollWheel") < -0.25f || Input.GetKey(KeyCode.Alpha1))
        {
            if (playerState == PlayerState.Editing)
            {
                CancelEdit();
            }
            playerState = PlayerState.Weapon;
            buildingScript.StopBuilding();
        }

        if (Input.GetKeyDown(firstSlotKeybind))
        {
            selectedItemInInventory = itemsInInventory[0];
        }
        else if (Input.GetKeyDown(secondSlotKeybind))
        {
            selectedItemInInventory = itemsInInventory[1];
        }
        else if (Input.GetKeyDown(thirdSlotKeybind))
        {
            selectedItemInInventory = itemsInInventory[2];
        }
        else if (Input.GetKeyDown(fourthSlotKeybind))
        {
            selectedItemInInventory = itemsInInventory[3];
        }
        else if (Input.GetKeyDown(fifthSlotKeybind))
        {
            selectedItemInInventory = itemsInInventory[4];
        }
    }

    void StartBuilding()
    {
        buildingScript.StartBuilding();
    }

    void CancelEdit()
    {
        Editing.Instance.CancelEdit();

    }

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
}
