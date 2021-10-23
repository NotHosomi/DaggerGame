using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : MonoBehaviour
{
    [SerializeField] GameObject dgr_fizzle;
    public DaggerController owner;
    protected Rigidbody2D rb;

    //bool hit;
    protected List<Vector2> hit_pos;
    protected List<Vector2> hit_norm;
    protected bool in_air = true;

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

        OnHit(collision);
    }

    protected virtual void OnHit(Collision2D collision)
    { }

    bool returning = false;
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

    public virtual void alt()
    {
        fizzleDagger();
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

    public float getWidth()
    {
        float w = 0;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if(box)
            w = box.size.x;
        CapsuleCollider2D cap = GetComponent<CapsuleCollider2D>();
        if (cap)
            if (cap.size.y > w)
                w = cap.size.y;
        CircleCollider2D cir = GetComponent<CircleCollider2D>();
        if (cir)
            if (cir.radius*2 > w)
                w = cap.size.y*2;
        return w;
    }
}
