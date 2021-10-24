using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : MonoBehaviour
{
    [SerializeField] GameObject dgr_fizzle;
    [SerializeField] float return_speed;
    [SerializeField] float return_start_speed;
    [SerializeField] float return_jolt;
    float return_time = 0;
    public DaggerController owner;
    protected Rigidbody2D rb;

    //bool hit;
    protected List<Vector2> hit_pos;
    protected List<Vector2> hit_norm;
    protected bool in_air = true;

    bool returning = false;
    public bool isReturning()
    {
        return returning;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (rb.velocity.sqrMagnitude > 0)
        {
            Vector2 vel = rb.velocity * (returning ? -1 : 1);
            transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg, Vector3.forward);
        }
        if (returning)
            return_time += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (returning)
        {
            moveToOwner();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Hit");
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

    public virtual void alt()
    {
        //fizzleDagger();

        if (returning)
            return;

        returning = true;
        rb.constraints = RigidbodyConstraints2D.None;
        in_air = true;
        rb.gravityScale = 0;
        foreach (Collider2D c in GetComponents<Collider2D>())
        {
            c.enabled = false;
        }

        rb.velocity = -transform.right * return_start_speed;
        Vector2 dir = moveToOwner();
        owner.GetComponent<Rigidbody2D>().velocity -= -dir * return_jolt; // jolt player
    }

    Vector2 moveToOwner()
    {
        Vector2 dir = owner.transform.position - transform.position;
        if (dir.SqrMagnitude() < 0.25f)
        {
            owner.collectDagger();
        }
        

        Vector2 vel = rb.velocity;
        float speed = (2* Mathf.Pow(2, -return_time * return_speed) + 2) * return_speed;
        vel = vel.normalized * speed;
        rb.velocity = Vector3.RotateTowards(vel, dir.normalized, 1.5f * Time.fixedDeltaTime * return_speed  , 0);
        //if (Vector2.Dot(rb.velocity, dir) > 1)
        //    owner.collectDagger();
        return dir;
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
