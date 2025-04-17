using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycloneStrikeBehaviour : MonoBehaviour
{
    [SerializeField] CharControl cc;
    [SerializeField] Animator anim;
    [SerializeField] GameObject tornado, slash;
    [SerializeField] float cycloneDamage, slashDamage;
    [SerializeField, Range(0f, 1f)] float slashAnimOffset;
    [SerializeField] Buster.Projectile cycloneDMGType,slashDMGType;
    float horizontalVelocity;
    bool travellingRight;
    AnimatorStateInfo stateInfo;
    void OnEnable()
    {
        travellingRight=cc.facingRight;
        tornado.SetActive(true);
        anim.SetLayerWeight(2,1);
        horizontalVelocity=0;
        anim.Play("Cyclone Strike",2,0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
        if ((collision.CompareTag("Boss") || collision.CompareTag("Enemy"))&&enemy)
        {
            enemy.TakeDamage(slash.activeSelf?slashDamage:cycloneDamage,(int)(slash.activeSelf?slashDMGType:cycloneDMGType));
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
        if ((collision.CompareTag("Boss") || collision.CompareTag("Enemy"))&&enemy&&tornado.activeSelf)
        {
            enemy.TakeDamage(cycloneDamage,(int)cycloneDMGType);
        }
    }
    void Update()
    {
        if (anim.GetLayerWeight(2)<=0||travellingRight!=cc.facingRight){gameObject.SetActive(false);}
        stateInfo = anim.GetCurrentAnimatorStateInfo(2);
        horizontalVelocity+=Time.deltaTime;
        if (stateInfo.normalizedTime>=1){anim.SetLayerWeight(2,anim.GetLayerWeight(2)-(Time.deltaTime*2));slash.SetActive(false);}
        else if (stateInfo.normalizedTime>=slashAnimOffset)
        {
            SlashAttack();
        }
    }
    public void SlashAttack()
    {
        tornado.SetActive(false);
        slash.SetActive(true);
        if (stateInfo.normalizedTime<slashAnimOffset){anim.Play("Cyclone Strike",2,slashAnimOffset);}
    }
    void LateUpdate()
    {
        if (stateInfo.normalizedTime<1)
        {
            cc.VelocityY=0;
            cc.VelocityX=horizontalVelocity*(travellingRight?1:-1);
        }
    }
    void OnDisable()
    {
        anim.SetLayerWeight(2,0);
        tornado.SetActive(false);
        slash.SetActive(false);
    }
}
