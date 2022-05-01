using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] int dmg = 1;
    [SerializeField] bool deflects = false;
    [SerializeField] bool deflects_self = false;
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 10);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Dagger d = collision.gameObject.GetComponent<Dagger>();
        if (d)
        {
            if (deflects)
            {
                Vector3 vel = d.GetComponent<Rigidbody2D>().velocity;
                vel.x *= -0.9f;
                d.GetComponent<Rigidbody2D>().velocity = vel;
            }
            if(deflects_self)
            {
                Vector3 vel = rb.velocity;
                vel.x *= -0.9f;
                rb.velocity = vel;
            }
            return;
        }
        Player p = collision.gameObject.GetComponent<Player>();
        if (p)
        {
            int x;
            if (rb.velocity.x < 0)
                x = -1;
            else
                x = 1;
            p.hurt(1, x);
            // TODO: activate a particle system
            Destroy(gameObject);
            return;
        }
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GetComponent<Collider2D>().enabled = false;
    }

    private void Update()
    {
        if (rb.velocity.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90, Vector3.forward);
        }
    }
}
