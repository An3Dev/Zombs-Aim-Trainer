using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editing : MonoBehaviour
{

    public GameObject editWall;
    public GameObject leftEdit, rightEdit, leftEditPressed, rightEditPressed;

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
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        Instance = this;
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
                leftEditPressed.SetActive(false);
                rightEdit.SetActive(true);
                rightEditPressed.SetActive(false);
                
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
                    
                    if (colliderName == "LeftEdit" && lastEdited != "LeftEditPress")
                    {
                        leftEdit.SetActive(false);
                        leftEditPressed.SetActive(true);
                        lastEdited = hit.collider.gameObject.name;
                    }
                    else if (colliderName == "RightEdit" && lastEdited != "RightEditPress")
                    {
                        rightEdit.SetActive(false);
                        rightEditPressed.SetActive(true);
                        lastEdited = hit.collider.gameObject.name;

                    }
                    else if (colliderName == "LeftEditPress" && lastEdited != "LeftEdit")
                    {
                        leftEditPressed.SetActive(false);

                        leftEdit.SetActive(true);
                        lastEdited = hit.collider.gameObject.name;

                    }
                    else if (colliderName == "RightEditPress" && lastEdited != "RightEdit")
                    {
                        rightEditPressed.SetActive(false);

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
                    rightEditPressed.SetActive(true);
                    rightEdit.SetActive(false);
                }

                if (rightWall.activeInHierarchy)
                {
                    leftEditPressed.SetActive(true);
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
        if ((leftEditPressed.activeInHierarchy && rightEditPressed.activeInHierarchy) || (leftEdit.activeInHierarchy && rightEdit.activeInHierarchy))
        {
            // showRegularWall
            regularWall.SetActive(true);

            leftWall.SetActive(false);
            rightWall.SetActive(false);
        }

        if (leftEditPressed.activeInHierarchy)
        {
            rightWall.SetActive(true);
            leftWall.SetActive(false);
            regularWall.SetActive(false);
        } else if (rightEditPressed.activeInHierarchy)
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
        leftEditPressed.SetActive(false);
        rightEditPressed.SetActive(false);
    }

    void PositionEditWall(Transform wall)
    {
        editWall.transform.position = wall.position;
        editWall.transform.rotation = wall.rotation;

        EnableEditingWall(true);
    }

    void EnableEditingWall(bool enable)
    {
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
