using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ECombat : MonoBehaviour
{
    [Header("Refs")]
    public Animator anim;
    public Rigidbody enemyRB;
    public Transform playerPos;
    public Transform pos;
    //public Transform atkPos;
    public ParticleSystem atkParticle;
    public EBehaviour eb;
    public PBehaviour pb;
    public LaunchRock lr;

    [Header("Position Values")]
    [SerializeField] private Vector3 difference;
    [SerializeField] private float distance;
    //private float activateDistance = 20f;

    [Header("Atk Values")]
    public int randomAtk;
    public List<attackType> atkTypes = new List<attackType>();
    //public float atkRangeE;
    public int atkDamageE;
    public float atkRateE;
    public float atkProjectileRateE;
    public float atkRadE;
    public float atkShootDelayE;
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
        enemyRB = GetComponent<Rigidbody>();
        playerPos = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        pos = GetComponent<Transform>();
        //atkParticle = atkPos.GetComponent<ParticleSystem>();
        eb = GetComponent<EBehaviour>();
        pb = GameObject.FindGameObjectWithTag("Player").GetComponent<PBehaviour>();
        lr = GameObject.FindGameObjectWithTag("RockSpawnPoint").GetComponent<LaunchRock>();

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
            
            } else if (distance > atkRadE)
            {
                if (distance < 25f && distance > 15f) // distance for the most accurate throws
                {
                    projectileAttack();
                    Debug.Log("jalan");
                    nextAtkTime = Time.time + 1f / atkProjectileRateE;
                } 
            }
        }
    }

    public void attack()
    {
        isAttacking = true;

        randomAtk = Random.Range(0, atkTypes.Count);
        string i = randomAtk.ToString();
        anim.SetTrigger("EAtk" + i);

        Invoke(nameof(attackHit), atkTypes[randomAtk].atkHitDelay);
        
        if (i == "2")
            enemyRB.isKinematic = false;
        else
            enemyRB.isKinematic = true;

        eb.rotSpeed = eb.rotSpeed * 0.005f;
        Invoke(nameof(resetAtk), atkTypes[randomAtk].atkHitDelay + atkTypes[randomAtk].isAtkingDelay);
    }

    public void attackHit() //ini bisa ditambahin param utk gonta ganti atkpos.pos, atkRange ,tp ntar pake ienum
    {
        atkTypes[randomAtk].vfx.Play();

        Collider[] hitPlayer;
        hitPlayer = Physics.OverlapSphere(atkTypes[randomAtk].atkPos.position, atkTypes[randomAtk].range, playerLayer);

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
        enemyRB.isKinematic = false;
        isAttacking = false;
    }

    public void projectileAttack()
    {
        isAttacking = true;
        lr.rockAtHand.SetActive(true);
        anim.SetTrigger("EAtkProjectile");
        //Invoke(nameof(resetRock), atkShootDelayE * 0.9f);
        Invoke(nameof(attackShoot), atkShootDelayE);

        enemyRB.isKinematic = true;
        eb.rotSpeed = eb.rotSpeed * 0.005f;
        Invoke(nameof(resetAtk), atkShootDelayE + isAttackingDelayE);
    }

    public void attackShoot()
    {
        // throw rock
        lr.rockAtHand.SetActive(false);
        lr.launchRock();
    }

    //public void resetRock()
    //{
    //    lr.rockAtHand.SetActive(false);
    //}

    private void OnDrawGizmosSelected()
    {
        if (atkTypes[randomAtk].atkPos == null)
            return;

        Gizmos.DrawWireSphere(atkTypes[randomAtk].atkPos.position, atkTypes[randomAtk].range);
    }

    [System.Serializable]
    public class attackType
    {
        public string name;
        public float damage;
        public float range;
        public float atkHitDelay;
        public float isAtkingDelay;
        public Transform atkPos;
        public ParticleSystem vfx;
    }
}
