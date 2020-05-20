using UnityEngine;
using Photon.Pun;
public class Building : MonoBehaviour
{

    float gridSpacing = 2.8f;

    bool isBuilding = false;

    Camera mainCamera;

    Vector3 tempWallPosition;
    Quaternion tempWallRotation;

    public GameObject wallPrefab;
    public GameObject wallPreview;

    public LayerMask buildsMask;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        
        wallPreview = GameObject.Find("WallPreview");
        wallPreview.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isBuilding)
        {
            tempWallPosition = PositionWall();

            RaycastHit2D hit = Physics2D.Linecast(transform.position, tempWallPosition, buildsMask);

            if (hit)
            {
                //Debug.Log(hit.transform.name);
                wallPreview.SetActive(false);
            } else
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
    }

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

    public void PlaceWall()
    {
        if (Physics2D.OverlapPoint(tempWallPosition, buildsMask))
        {
            return;
        }

        GameObject wall = PhotonNetwork.InstantiateSceneObject("Wall", tempWallPosition, tempWallRotation);
        //Instantiate(wallPrefab, tempWallPosition, tempWallRotation);
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

        RaycastHit2D hit = Physics2D.Linecast(transform.position, temp, buildsMask);


        Vector3 finalDirPoint = (hit ? new Vector3(hit.point.x, hit.point.y, 0) : temp);

        //Debug.DrawLine(transform.position, finalDirPoint, Color.grey);

        //Debug.DrawLine(transform.position, temp, Color.blue);

        finalPosition = GetGridPosition(finalDirPoint);

        //Debug.DrawLine(transform.position, finalPosition, Color.red);

        // if placing horizontal wall
        if (tempWallRotation.eulerAngles != Vector3.zero)
        {
            //if (finalPosition.x > GetGridPosition(transform.position).x)
            //{
            //    //Debug.Log("Right");
            //    finalPosition.x -= gridSpacing / 2;

            //}
            //else if (finalPosition.x < GetGridPosition(transform.position).x)
            //{
            //    //Debug.Log("Left");
            //    finalPosition.x += gridSpacing / 2;
            //}

            //if (finalPosition.y > GetGridPosition(transform.position).y)
            //{
            //    //Debug.Log("Up");
            //    finalPosition.y -= gridSpacing / 2;
            //}
            //else if (finalPosition.y < GetGridPosition(transform.position).y)
            //{
            //    //Debug.Log("Down");
            //    finalPosition.y += gridSpacing / 2;
            //}
            //else if (finalPosition.y == GetGridPosition(transform.position).y)
            //{
            //    //Debug.Log("Equal");
            //}
        }
        //Debug.Log(finalPosition);

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
}
