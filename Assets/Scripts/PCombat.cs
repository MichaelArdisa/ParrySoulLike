using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PCombat : MonoBehaviour
{
    [Header("Refs")]
    public GameObject shieldEffect;
    public ParticleSystem slashEffect;
    public PMove pm;
    public GameObject player;
    public Transform aimPos;

    [Header("Parry Values")]
    public float parryCD;
    public bool canParryM;

    private Vector3 orShieldScale;
    private float orSpeed;
    private float orDodgeSpeed;
    private float orRotSmoothTime;

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
    }

    // Update is called once per frame
    void Update()
    {
        parryMelee();
        parryProjectile();
    }

    void parryMelee()
    {
        if (Input.GetButton("ParryMelee") && canParryM && pm.isGround)
        {
            shieldEffect.transform.DOScale(orShieldScale, 0.04f);

            // add parrying animation
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
            shieldEffect.transform.DOScale(Vector3.zero, 0.5f);
            slashEffect.Play();
            
            canParryM = false;

            Invoke(nameof(speedReset), 0.25f);
            Invoke(nameof(parryMReset), parryCD);
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
    }

    void parryProjectile()
    {
        if (Input.GetButton("ParryProjectile") && canParryM && pm.isGround)
        {

        }
    }
}
