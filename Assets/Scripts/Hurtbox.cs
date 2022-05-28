using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    [SerializeField] int dmg = 1;
    [SerializeField] bool return_to_safety = false;
    [SerializeField] bool deflects = false;
    [SerializeField] bool deflects_self = false;

    Rigidbody2D rb;

    private void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Dagger d = other.GetComponent<Dagger>();
        if (d)
        {
            if (deflects_self && rb != null)
            {
                Vector3 vel = rb.velocity;
                vel *= 1.5f;
                vel.x *= -1f;
                rb.velocity = vel;
                if (rb.gravityScale == 0)
                    rb.gravityScale = 1;
            }
            if (deflects)
            {
                Vector3 vel = d.GetComponent<Rigidbody2D>().velocity;
                vel *= 0.25f;
                vel.x *= -1f;
                d.GetComponent<Rigidbody2D>().velocity = vel;
                //d.deflect;
            }
            else
                d.owner.collectDagger(d.gameObject);
            // take damage
            return;
        }
        Player p = other.GetComponent<Player>();
        if (p)
        {            
            if (return_to_safety)
            {
                p.hitHazard();
                return;
            }
            int dir = (int)Mathf.Sign(transform.position.x - p.transform.position.x);
            p.hurt(dmg, dir);
        }
    }
}
