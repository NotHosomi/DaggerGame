using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : MonoBehaviour
{
    [SerializeField] GameObject dgr_fizzle;
    public DaggerController owner;
    Rigidbody2D rb;

    //bool hit;
    List<Vector2> hit_pos;
    List<Vector2> hit_norm;
    bool returning = false;
    bool in_air = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(rb.velocity.sqrMagnitude > 0)
        {
            Vector2 vel = rb.velocity;
            transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        in_air = false;
        
        Vector2 pos = transform.position;
        hit_pos = new List<Vector2>();
        hit_norm = new List<Vector2>();
        for (int i = 0; i < collision.contactCount; ++i)
        {
            hit_pos.Add(collision.GetContact(i).point - pos);
            hit_norm.Add(collision.GetContact(i).normal);
            Debug.DrawRay(hit_pos[i], hit_norm[i], Color.blue, 5);
        }
    }

    public void pullBack()
    {
        if(returning)
        {
            Destroy(this);
            owner.collectDagger();
        }
        in_air = true;
        rb.constraints = RigidbodyConstraints2D.None;
        returning = true;
    }

    public Vector2 getHitPos(int i = 0)
    {
        return hit_pos[i];
    }
    public Vector2 getHitNorm(int i = 0)
    {
        return hit_norm[i];
    }
    public List<Vector2> getHitPosList()
    {
        return hit_pos;
    }
    public List<Vector2> getHitNormList()
    {
        return hit_norm;
    }

    public void fizzleDagger()
    {
        Destroy(this);
        owner.collectDagger();
        // instatiate death particles, and give them our velocity
        Instantiate(dgr_fizzle, transform.position, transform.rotation).GetComponent<Rigidbody2D>().velocity = rb.velocity * 0.3f;
    }

    public bool isLanded()
    {
        return !in_air;
    }
}
