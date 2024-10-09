using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBehaviour : MonoBehaviour
{

    [Header("Refs")]
    public GameObject enemy;
    public Transform enemyPos;
    public Transform playerPos;
    //public HealthBar healthBar;
    public Animator anim;
    //public Material dissolveMaterial;
    //public ToDoBehaviour toDo;
    //public Collider coll;

    [Header("Enemy Values")]
    //public int maxHP;
    //private int currHP;
    //public float speed;
    public int dirDeg;
    public float rotSpeed;
    //private float blinkDuration = 0.15f;

    [Header("Validations")]
    public bool isHit;
    //public bool isMalware;
    //public bool isAdware;
    //public bool isPopUp;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        enemyPos = GetComponent<Transform>();
        playerPos = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // Enemy Movement
        Vector3 dirToPlayer = playerPos.position - enemyPos.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer != Vector3.zero)
        {
            anim.SetTrigger("orcWalk");

            Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        
        } else
        {
            anim.SetTrigger("orcIdle");
        }
    }
}
