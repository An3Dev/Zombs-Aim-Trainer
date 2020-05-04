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
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;   
    }

    // Update is called once per frame
    void Update()
    {
        if (isEditingWall)
        {

            if (Input.GetMouseButtonUp(0))
            {
                lastEdited = "";
            }

            if (Input.GetMouseButton(0))
            {
                // edit
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
                Vector3 direction = (mouseWorldPos - transform.root.position).normalized;

                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 5, wallMask);

                Debug.DrawLine(transform.position, transform.position + direction);

                string colliderName = hit.collider.name;

                if (colliderName == "LeftEdit" && lastEdited != "LeftEditPress")
                {
                    leftEdit.SetActive(false);
                    leftEditPressed.SetActive(true);
                    lastEdited = hit.collider.gameObject.name;
                } else if (colliderName == "RightEdit" && lastEdited != "RightEditPress")
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

    public bool Edit()
    {
        if (isEditingWall)
        {
            isEditingWall = false;

            // confirm
        }
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
        Vector3 direction = (mouseWorldPos - transform.root.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 5, wallMask);
        
        
        if (hit)
        {            
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("Edit");
                isEditingWall = true;
                PositionEditWall(hit.collider.transform);
                return true;
            }
        }

        return false;
    }
    
    void Confirm()
    {
        if (leftEditPressed.activeInHierarchy)
        {
            disabledWall.Get
        }
    }

    void PositionEditWall(Transform wall)
    {
        editWall.transform.position = wall.position;
        editWall.transform.rotation = wall.rotation;

        disabledWall = wall.gameObject;

        EnableEditingWall(true);
    }

    void EnableEditingWall(bool enable)
    {
        disabledWall.gameObject.SetActive(!enable);

        editWall.SetActive(enable);
    }
}
