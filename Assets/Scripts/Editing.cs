using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editing : MonoBehaviour
{

    public GameObject editWall;
    public GameObject leftEdit, rightEdit, leftEditPress, rightEditPress;

    bool isEditingWall = false;

    public LayerMask wallMask;

    GameObject disabledWall;

    Camera mainCamera;

    string lastEdited;

    public static Editing Instance;

    GameObject leftWall, rightWall, regularWall;

    public bool editOnRelease;
    public bool resetOnRelease = true;

    public float editDistance;
    
    void Awake()
    {
        mainCamera = Camera.main;
        Instance = this;

        editWall = GameObject.Find("EditWall");
        editWall.SetActive(true);
        leftEdit = GameObject.Find("LeftEdit");
        rightEdit = GameObject.Find("RightEdit");
        leftEditPress = GameObject.Find("LeftEditPress");
        rightEditPress = GameObject.Find("RightEditPress");


        leftEditPress.SetActive(false);
        rightEditPress.SetActive(false);
        editWall.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
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

            if (Input.GetMouseButton(0))
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
                    Debug.Log(colliderName);
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
        Debug.Log("Confirm");
        for (int i = 0; i < disabledWall.transform.childCount; i++)
        {
            if (disabledWall.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                disabledWall.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        // if both are pressed, don't edit and show regular wall
        if ((leftEditPress.activeInHierarchy && rightEditPress.activeInHierarchy) || (leftEdit.activeInHierarchy && rightEdit.activeInHierarchy))
        {
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
        } else if (rightEditPress.activeInHierarchy)
        {
            leftWall.SetActive(true);
            rightWall.SetActive(false);
            regularWall.SetActive(false);
        }

        ResetEditWall();
        editWall.SetActive(false);
        disabledWall.SetActive(true);
        Movement.Instance.StoppedEditing();

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
                disabledWall.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = !enable;
            }
        }
        //disabledWall.gameObject.SetActive(!enable);

        editWall.SetActive(enable);
    }
}
