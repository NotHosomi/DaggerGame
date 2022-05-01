using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    [SerializeField] int dmg = 1;
    [SerializeField] bool return_to_safety = false;
    [SerializeField] bool deflects = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Dagger d = other.GetComponent<Dagger>();
        if (d)
        {
            if(deflects)
            {
                Vector3 vel = d.GetComponent<Rigidbody2D>().velocity;
                vel.x *= -0.9f;
                d.GetComponent<Rigidbody2D>().velocity = vel;
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
            // TODO: find knockback dir
            p.hurt(dmg);
            //p.GetComponent<Rigidbody2D>().AddForce(collision.GetContact(0).)
        }
    }
}
