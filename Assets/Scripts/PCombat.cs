using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PCombat : MonoBehaviour
{
    [Header("Refs")]
    public GameObject shieldEffect;
    public ParticleSystem slashEffect;
    public ParticleSystem projectileParrySlash;
    public PMove pm;
    public EBehaviour eb;
    public GameObject player;
    //public GameObject Enemy;
    public Transform atkPos;
    public Transform aimPos;
    public Animator anim;
    //public GameObject actualSword;
    //public GameObject backSword;

    [Header("Atk Values")]
    public float atkRate;
    public List<combo> combos = new List<combo>();
    public float atkRange;
    public float atkHitDelay;
    public float isAttackingDelay;
    public int comboIndex = 0;

    [Header("Parry Values")]
    public float parryMCD;
    public float parryPCD;
    public bool canParryM;
    public bool canParryP;
    public float parryMeleeHitDelay;
    public float projectileParryForce;

    [Header("Validations")]
    public LayerMask enemyLayer;
    public LayerMask projectileLayer;
    public bool isAttacking;
    public bool isComboing;

    private Vector3 orShieldScale;
    private float nextAtkTime = 0f;
    private float orSpeed;
    private float orDodgeSpeed;
    private float orRotSmoothTime;
    [SerializeField] private float comboTimer;

    // Start is called before the first frame update
    void Start()
    {
        shieldEffect = GameObject.FindGameObjectWithTag("ShieldEffect");
        pm = GetComponent<PMove>();
        player = GameObject.FindGameObjectWithTag("Player");

        orShieldScale = shieldEffect.transform.localScale;
        shieldEffect.transform.localScale = Vector3.zero;
        //shieldEffect.transform.DOScale(orShieldScale / 1000, 0.5f);

        orSpeed = pm.pSpeed;
        orDodgeSpeed = pm.dodgeSpeed;
        orRotSmoothTime = pm.rotSmoothTime;
        canParryM = true;
        canParryP = true;
    }

    // Update is called once per frame
    void Update()
    {
        parryMelee();
        parryProjectile();

        if (Time.time >= nextAtkTime && pm.isGround && !pm.isDodging && !isAttacking)
            if (Input.GetButtonDown("normalAttack"))
            {
                normalAttack();
                nextAtkTime = Time.time + 1f / atkRate;
            }

        //Debug.Log(anim.GetCurrentAnimatorStateInfo(0).IsName("Atk" + "0"));

        if (isComboing)
            comboTimer = comboTimer + Time.deltaTime;

        if (comboTimer > 0.5f)
        {
            comboIndex = 0;
            isComboing = false;
            comboTimer = 0;
        }
    }

    void normalAttack()
    {
        isComboing = true;

        if (comboIndex >= combos.Count)
        {
            comboIndex = 0;
            isComboing = false;
            comboTimer = 0;
            isAttacking = false;

            Invoke(nameof(isAttackingReset), 10f);
        }

        //Debug.Log(comboIndex);
        isAttacking = true;

        string i = comboIndex.ToString();
        anim.SetTrigger("Atk" + i);
        combos[comboIndex].slashEffect.Play();
        comboIndex++;
        comboTimer = 0;

        Invoke(nameof(attackHit), atkHitDelay);

        pm.pSpeed = pm.pSpeed * 0.05f;
        pm.rotSmoothTime = pm.rotSmoothTime + 1f;
        Invoke(nameof(resetAtk), atkHitDelay + isAttackingDelay);
    }

    void attackHit()
    {
        Collider[] hitEnemy;
        hitEnemy = Physics.OverlapSphere(atkPos.position, atkRange, enemyLayer);

        foreach (Collider enemy in hitEnemy)
        {
            Debug.Log(enemy.name + " hit " + "with " + combos[comboIndex - 1].name);
            eb = enemy.GetComponent<EBehaviour>();
            //eb.ETakeDamage(atkDamageE);
            //Debug.Log(++count);
        }
    }

    void resetAtk()
    {
        pm.rotSmoothTime = orRotSmoothTime;
        pm.pSpeed = orSpeed;
        isAttackingReset();
    }

    void isAttackingReset()
    {
        isAttacking = false;
    }

    void parryMelee()
    {
        if (Input.GetButton("ParryMelee") && canParryM && pm.isGround)
        {
            isAttacking = true;

            shieldEffect.transform.DOScale(orShieldScale, 0.04f);
            anim.SetTrigger("ParryM0");

            pm.canJump = false;
            pm.pSpeed = 0f;
            pm.dodgeSpeed = 0f;
            pm.rotSmoothTime = 3f;
            pm.canDodge = false;
            pm.isGround = false;

            // aims player to what the camera is aiming at
            Vector3 dirToAim = aimPos.position - transform.position;
            dirToAim.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(dirToAim, Vector3.up);
            transform.rotation = targetRotation;
        }

        if (Input.GetButtonUp("ParryMelee") && canParryM && pm.isGround)
        {
            pm.canJump = true;
            canParryM = false;

            anim.SetTrigger("ParryM1");

            Invoke(nameof(parryHit), parryMeleeHitDelay);
            Invoke(nameof(speedReset), 0.25f);
            Invoke(nameof(parryMReset), parryMCD);
        }
    }

    void parryHit()
    {
        slashEffect.Play();
        shieldEffect.transform.DOScale(Vector3.zero, 0.5f);

        Collider[] hitEnemy;
        hitEnemy = Physics.OverlapSphere(atkPos.position, atkRange * 1.5f, enemyLayer);

        foreach (Collider enemy in hitEnemy)
        {
            Debug.Log(enemy.name + " hit ");
            eb = enemy.GetComponent<EBehaviour>();
            //eb.ETakeDamage(atkDamageE);
            //Debug.Log(++count);
        }
    }

    void speedReset()
    {
        pm.pSpeed = orSpeed;
        pm.dodgeSpeed = orDodgeSpeed;
        pm.rotSmoothTime = orRotSmoothTime;
        pm.canDodge = true;
        pm.isGround = true;
    }

    void parryMReset()
    {
        canParryM = true;
        isAttacking = false;
    }

    void parryProjectile()
    {
        if (Input.GetButtonDown("ParryProjectile") && canParryP && pm.isGround)
        {
            isAttacking = true;
            canParryP = false;

            anim.SetTrigger("ParryP");
            projectileParrySlash.Play();
            projectileParry();

            pm.pSpeed = 0f;
            pm.dodgeSpeed = 0f;
            pm.rotSmoothTime = 3f;
            pm.canDodge = false;
            pm.isGround = false;

            Invoke(nameof(speedReset), 0.1f);
            Invoke(nameof(parryPReset), parryPCD);
        }
    }

    void parryPReset()
    {
        canParryP = true;
        isAttacking = false;
    }

    void projectileParry()
    {
        Collider[] hitProjectile;
        hitProjectile = Physics.OverlapSphere(atkPos.position, atkRange, projectileLayer);

        foreach (Collider projectile in hitProjectile)
        {
            Debug.Log(projectile.name + " hit ");
            projectile.attachedRigidbody.AddForce(-projectile.attachedRigidbody.velocity * projectileParryForce, ForceMode.Impulse);
        }

    }

    [System.Serializable]
    public class combo
    {
        public string name;
        public float damage;
        public ParticleSystem slashEffect;
    }

    private void OnDrawGizmosSelected()
    {
        if (atkPos == null)
            return;

        Gizmos.DrawWireSphere(atkPos.position, atkRange);
    }
}
