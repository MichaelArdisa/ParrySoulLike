using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRock : MonoBehaviour
{
    public GameObject rock;
    public Rigidbody rb;
    public Transform player;
    public float launchForce;

    // Start is called before the first frame update
    void Start()
    {
        rock = GetComponent<GameObject>();
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        Vector3 dirToPlayer = player.position - transform.position;

        //rb.AddForce(dirToPlayer * launchForce, ForceMode.Impulse);
        rb.AddForce(new Vector3(dirToPlayer.x * launchForce, dirToPlayer.y + 4f, dirToPlayer.z * launchForce), ForceMode.Impulse);
    }
}
