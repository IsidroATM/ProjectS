using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public Rigidbody platformRB;
    public Transform[] platformPositions;
    public float platformSpeed;

    private int actualPos = 0;
    private int nextPos = 1;

    public bool moveToTheNext = true;
    public float waitTime;

    void Update()
    {
        MovePlatform();
    }
    void MovePlatform()
    {
        if (moveToTheNext)
        {
            StopCoroutine(WaitForMove(0));
            platformRB.MovePosition(Vector3.MoveTowards(platformRB.position, platformPositions[nextPos].position, platformSpeed * Time.deltaTime));
        }
        
        if (Vector3.Distance(platformRB.position, platformPositions[nextPos].position) <= 0)
        {
            StartCoroutine(WaitForMove(waitTime));
            actualPos = nextPos;
            nextPos++;
            if (nextPos >platformPositions.Length - 1)
            {
                nextPos = 0;
            }
        }
    }
    IEnumerator WaitForMove(float time)
    {
        moveToTheNext = false;
        yield return new WaitForSeconds(time);
        moveToTheNext = true;
    }
}
