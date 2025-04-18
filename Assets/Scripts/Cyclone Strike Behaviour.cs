using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycloneStrikeBehaviour : MonoBehaviour
{
    [SerializeField] CharControl cc;
    [SerializeField] Animator anim;
    [SerializeField] GameObject tornado, slash, slashTrail;
    [SerializeField] float cycloneDamage, slashDamage;
    [SerializeField, Range(0f, 1f)] float slashAnimOffset;
    [SerializeField] Buster.Projectile cycloneDMGType,slashDMGType;
    float horizontalVelocity=6;
    bool travellingRight;
    AnimatorStateInfo stateInfo;
    void OnEnable()
    {
        travellingRight=cc.facingRight;
        tornado.SetActive(true);
        anim.SetLayerWeight(2,1);
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
        stateInfo = anim.GetCurrentAnimatorStateInfo(2);
        if (anim.GetLayerWeight(2)<=0||travellingRight!=cc.facingRight){gameObject.SetActive(false);}
        else if (stateInfo.normalizedTime>=1){anim.SetLayerWeight(2,Mathf.Clamp(anim.GetLayerWeight(2)-(Time.deltaTime*2),0,1));slash.SetActive(false);slashTrail.SetActive(false);}
        else if (stateInfo.normalizedTime>=0.9f){slashTrail.SetActive(true);}
        else if (stateInfo.normalizedTime>=slashAnimOffset)
        {
            tornado.SetActive(false);
            slash.SetActive(true);
        }
    }
    public void SlashAttack()
    {
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
