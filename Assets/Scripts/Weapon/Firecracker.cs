using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firecracker : MonoBehaviour
{
    [SerializeField] float speed=12,damage, armTime, ExplosionInterval, fizzleOutTimeAfterArming;
    [SerializeField] GameObject Explosion;
    [SerializeField] Buster.Projectile damageType;
    [SerializeField] AudioClip armingSound;
    Rigidbody2D rb;
    Vector3 scale;
    float timer=0;
    float fullDamage;
    bool Armed=false;
    void Start()
    {
        rb = gameObject.GetAny<Rigidbody2D>();
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
        if (timer>(armTime))
        {
            if (timer>(armTime+fizzleOutTimeAfterArming)){Destroy(gameObject);}
            transform.localScale = scale*(1-((timer-armTime)/fizzleOutTimeAfterArming));
            damage = fullDamage*(1-((timer-armTime)/fizzleOutTimeAfterArming));
        }
        if (timer>=armTime&&!Armed)
        {
            Armed=true;
            InvokeRepeating("GoBoom",0,ExplosionInterval);
            Invoke("FizzleOut",fizzleOutTimeAfterArming);
        }
    }
    void GoBoom()
    {
        float volume = GetComponent<AudioSource>().volume;
        if (timer>=armTime)
        {
            GameObject Boom = Instantiate(Explosion,transform.position,Quaternion.identity);
            ExplosionBehavior BoomBehaviour = Boom.GetAny<ExplosionBehavior>();
            Boom.transform.localScale*=2.5f;
            BoomBehaviour.volume=volume;
            BoomBehaviour.damage=damage;
            BoomBehaviour.friendlyFire=false;
            BoomBehaviour.playerOwned=true;
            BoomBehaviour.damageType=damageType;
            BoomBehaviour.destroyEnvironment=true;
        }
    }
    void FizzleOut()
    {
        Destroy(gameObject);
    }
}
