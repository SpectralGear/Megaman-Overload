using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkBomb : MonoBehaviour
{
    [SerializeField] float speed=12,damage, armTime, fullPowerGracePeriod, fizzleOutTimeAfterArming;
    [SerializeField] GameObject Explosion;
    [SerializeField] Buster.Projectile damageType;
    [SerializeField] ParticleSystem armIndicatorFlash;
    [SerializeField] AudioClip armingSound;
    Rigidbody2D rb;
    Vector3 scale;
    float timer=0;
    float fullDamage;
    bool flashPlayed=false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        scale = transform.localScale;
        fullDamage = damage;
    }
    private void FixedUpdate()
    {
        float angleInRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 moveDirection = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        rb.velocity = moveDirection.normalized * speed;
    }
    void Update()
    {
        timer+=Time.deltaTime;
        if (timer>(armTime+fullPowerGracePeriod))
        {
            if (timer>(armTime+fullPowerGracePeriod+fizzleOutTimeAfterArming)){Destroy(gameObject);}
            transform.localScale = scale*(1-((timer-armTime-fullPowerGracePeriod)/fizzleOutTimeAfterArming));
            damage = fullDamage*(1-((timer-armTime-fullPowerGracePeriod)/fizzleOutTimeAfterArming));
        }
        if (timer>=armTime&&!flashPlayed){armIndicatorFlash.Play(true);GetComponent<AudioSource>().PlayOneShot(armingSound);flashPlayed=true;}
    }
    public void GoBoom()
    {
        if (timer>=armTime)
        {
            GameObject Boom = Instantiate(Explosion,transform.position,Quaternion.identity);
            ExplosionBehavior BoomBehaviour = Boom.GetComponent<ExplosionBehavior>();
            Boom.transform.localScale*=2;
            BoomBehaviour.damage=damage;
            BoomBehaviour.friendlyFire=false;
            BoomBehaviour.playerOwned=true;
            BoomBehaviour.damageType=damageType;
            BoomBehaviour.destroyEnvironment=timer<=(armTime+fullPowerGracePeriod);
            Destroy(gameObject);
        }
    }
}
