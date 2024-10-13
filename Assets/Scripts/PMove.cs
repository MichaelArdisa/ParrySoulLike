using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;

public class PMove : MonoBehaviour
{
    [Header("Refs")]
    public CharacterController ctrl;
    public Transform groundCheck;
    public Transform camPos;
    public Animator anim;
    public PCombat pc;
    public GameObject actualSword;
    public GameObject backSword;

    [Header("Player Values")]
    public float pSpeed;
    public float pJumpHeight;
    public Vector3 direction;
    private float rotSpeed;
    public float rotSmoothTime = 0.1f;
    public float gravity;

    [Header("Ground Check")]
    public bool isGround;
    public bool canJump;
    //public float checkerSize;
    public Vector3 checkerSize;
    public LayerMask groundMask;

    [Header("Dodge Check")]
    public bool isDodging;
    public bool isDodgingCont;
    public bool canDodge;
    public int dodgeCount;
    public int dodgeLimit;
    public float dodgeDistance;
    public float dodgeCD;
    public float dodgeTimer;
    public float dodgeSpeed;
    public float dodgeAdjuster1;
    public float dodgeAdjuster2;
    //public Vector2 dodgeDir;

    [Header("Speed")]
    [SerializeField] private Vector3 velo;
    [SerializeField] private Vector3 speed;

    private Vector3 move;

    // Start is called before the first frame update
    void Start()
    {
        ctrl = GetComponent<CharacterController>();
        camPos = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        pc = GetComponent<PCombat>();

        Cursor.lockState = CursorLockMode.Locked;
        isDodging = false;
        canDodge = true;
        canJump = true;
    }

    // Update is called once per frame
    void Update()
    {
        //isGround = Physics.CheckSphere(groundCheck.position, checkerSize, groundMask);
        isGround = Physics.CheckBox(groundCheck.position, checkerSize, this.transform.rotation, groundMask);

        if (isGround && velo.y < 0f)
            velo.y = -5f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        direction = new Vector3(x, 0f, z).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Player rotation & Cam angle
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camPos.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotSpeed, rotSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            //Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);

            // Move
            anim.SetTrigger("Run");
            ctrl.Move(moveDir.normalized * pSpeed * Time.deltaTime);
            speed = moveDir * pSpeed;
        }
        else if (direction.magnitude <= 0.1f)
            anim.SetTrigger("Idle");

        // Jump
        if (isGround && canJump && Input.GetButtonDown("Jump"))
        {
            velo.y = Mathf.Sqrt(pJumpHeight * -2f * gravity);
            //canJump = false;

            anim.SetTrigger("Jump");
        }

        velo.y += gravity * Time.deltaTime;
        ctrl.Move(velo * Time.deltaTime);

        // Dodge
        if (isDodgingCont)
            dodgeTimer = dodgeTimer + Time.deltaTime;

        if (!isDodging && canDodge && isGround && dodgeCount > 0 && Input.GetButtonDown("Dodge"))
        {
            isDodgingCont = true;
            
            dodgeCount--;
            dodgeTimer = 0;

            anim.SetTrigger("Dodge");

            canDodge = false;
            isDodging = true;
            Vector3 dodge = transform.forward;

            StartCoroutine(dodgeMove(dodge, dodgeDistance));
            Invoke(nameof(dodgeReset), dodgeCD);
        }

        if (dodgeTimer > 1.5f && dodgeTimer < 2f)
        {
            dodgeTimer = 3f; // to make it so that dodgeIncrement only runs once despite being inside of update
            StartCoroutine(dodgeIncrement());
            isDodgingCont = false;
        }

        // Sword validation
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Run") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Dodge") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            turnOnBackSword();
        else
            turnOnActualSword();
    }

    IEnumerator dodgeMove(Vector3 dodge, float dodgeDist)
    {
        dodge = dodge * dodgeSpeed;
        float count = 0;

        while (count < dodgeDist)
        {
            ctrl.Move(dodge);
            count = count + dodgeAdjuster1;
            yield return new WaitForSeconds(dodgeAdjuster2);
        }

        isDodging = false;
    }

    IEnumerator dodgeIncrement()
    {
        while (dodgeCount < dodgeLimit)
        {
            if (dodgeCount < dodgeLimit)
                dodgeCount++;

            yield return new WaitForSeconds(1f);
        }
    }

    void dodgeReset()
    {
        ctrl.Move(move * pSpeed * Time.deltaTime);
        canDodge = true;
    }

    public void turnOnActualSword()
    {
        actualSword.SetActive(true);
        //add animation
        backSword.SetActive(false);
    }

    public void turnOnBackSword()
    {
        backSword.SetActive(true);
        // add animation
        actualSword.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        //Gizmos.DrawWireSphere(groundCheck.position, checkerSize);
        //Gizmos.DrawLine(this.transform.position, this.transform.position);

        Gizmos.DrawWireCube(groundCheck.position , checkerSize);
    }
}
