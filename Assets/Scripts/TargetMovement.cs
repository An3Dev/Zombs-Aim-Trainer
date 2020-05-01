using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{

    float timeTillMove;
    Vector2 timeTillMoveRange = new Vector2(1f, 7f);

    float lastMoveTime;

    Vector3 direction;

    Vector2 speedRange = new Vector2(1, 2);
    float speed;

    // Start is called before the first frame update
    void Start()
    {
        AssignNewValues();
    }

    void AssignNewValues()
    {
        timeTillMove = Random.Range(timeTillMoveRange.x, timeTillMoveRange.y);
        speed = Random.Range(speedRange.x, speedRange.y);
        direction = new Vector3(Random.Range(0.1f, 1), Random.Range(0.1f, 1), 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (SpawnTargets.Instance.startGame)
        {
            if (lastMoveTime == 0)
            {
                lastMoveTime = Time.timeSinceLevelLoad;
            }
        }

        if (!SpawnTargets.Instance.startGame) return;

        if (Time.timeSinceLevelLoad - lastMoveTime >= timeTillMove)
        {
            AssignNewValues();
            lastMoveTime = Time.timeSinceLevelLoad;
            Debug.Log(direction);
        }

        transform.position += direction * speed * Time.deltaTime;
        if (transform.position.y >= 5 || transform.position.y <= -5 || transform.position.x >= 10 || transform.position.x <= -10)
        {
            direction = -direction;
        }
    }


}
