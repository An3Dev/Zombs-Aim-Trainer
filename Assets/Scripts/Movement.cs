using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    bool confirmedEditThisFrame = false;
    // Start is called before the first frame update
    void Start()
    {
        activeShootScript = GetComponentInChildren<Shoot>();
        buildingScript = GetComponent<Building>();
        editingScript = GetComponent<Editing>();
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
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
                activeShootScript.ShootWeapon();
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
            //if (confirmedEditThisFrame)
            //{
            //    confirmedEditThisFrame = false;
            //    Debug.Log("Confirmed this frame");
            //    return;
            //}

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
        confirmedEditThisFrame = true;
    }
    void Rotate()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
        Vector3 direction = (mouseWorldPos - transform.position).normalized;
        transform.up = direction;
    }
}
