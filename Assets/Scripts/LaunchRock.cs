using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchRock : MonoBehaviour
{
    public Transform launchPoint;
    public GameObject rockAtHand;
    public GameObject rock;

    private void Start()
    {
        launchPoint = GetComponent<Transform>();
        rockAtHand.SetActive(false);
    }

    public void launchRock()
    {
        Instantiate(rock, launchPoint.position, transform.rotation);
    }
}
