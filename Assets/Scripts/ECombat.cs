using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ECombat : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;
    public Transform playerPos;
    public Transform pos;
    public Transform atkPos;
    public EBehaviour eb;
    public PBehaviour pb;

    [Header("Position Values")]
    [SerializeField] private Vector3 difference;
    [SerializeField] private float distance;
    //private float activateDistance = 20f;

    [Header("Atk Values")]
    public float atkRangeE;
    public int atkDamageE;
    public float atkRateE;
    public float atkRadE;
    public float atkDelayE;
    public float isAttackingDelayE;

    [Header("Validations")]
    public bool isBoss;
    public bool isAttacking;

    public LayerMask playerLayer;

    private float nextAtkTime = 0f;
    //private float orSpeed;
    private float orRotSpeed;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        playerPos = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        pos = GetComponent<Transform>();
        eb = GetComponent<EBehaviour>();
        pb = GameObject.FindGameObjectWithTag("Player").GetComponent<PBehaviour>();

        //orSpeed = eb.speed;
        orRotSpeed = eb.rotSpeed;
        isAttacking = false;
    }

    // Update is called once per frame
    void Update()
    {
        difference = playerPos.position - pos.position;
        distance = difference.magnitude;

        if (Time.time >= nextAtkTime && isBoss && !isAttacking)
        {
            if (distance < atkRadE)
            {
                attack();
                nextAtkTime = Time.time + 1f / atkRateE;
            }
        }
    }

    public void attack()
    {
        isAttacking = true;

        anim.SetTrigger("orcAtk1");
        Invoke(nameof(attackHit), atkDelayE);

        eb.rotSpeed = eb.rotSpeed * 0.05f;
        Invoke(nameof(resetAtk), atkDelayE + isAttackingDelayE);
    }

    public void attackHit() //ini bisa ditambahin param utk gonta ganti atkpos.pos, atkRange ,tp ntar pake ienum
    {
        Collider[] hitPlayer;
        hitPlayer = Physics.OverlapSphere(atkPos.position, atkRangeE, playerLayer);

        foreach (Collider player in hitPlayer)
        {
            Debug.Log(player.name + " hit ");
            pb = player.GetComponent<PBehaviour>();
            //pb.PTakeDamage(atkDamageE);
            //Debug.Log(++count);
        }
    }

    public void resetAtk()
    {
        eb.rotSpeed = orRotSpeed;
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (atkPos == null)
            return;

        Gizmos.DrawWireSphere(atkPos.position, atkRangeE);
    }
}
