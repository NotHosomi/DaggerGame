using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyGeneric : MonoBehaviour
{
    public Transform target;
    Vector2 home_pos;
    Bounds wander_zone;


    public float speed = 200f;
    public float nextWaypointDistance = 3f;
    public float max_follow_dist = 10f;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        if(transform.parent != null)
            wander_zone = transform.parent.GetComponent<BoxCollider2D>().bounds;

        home_pos = transform.position;

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        if (!seeker.IsDone())
            return;

        Vector2 t = home_pos;

        if ((rb.position - (Vector2)target.position).sqrMagnitude < max_follow_dist * max_follow_dist)// &&
            //!Physics2D.Linecast(rb.position, target.position, LayerMask.GetMask("Default")))
            t = target.position;
        else if (transform.parent != null)
        {
            if (!reachedEndOfPath)
                return;
            if (wander_zone.Contains(rb.position))
                t = (Vector2)wander_zone.center + new Vector2(
                    Random.Range(wander_zone.min.x, wander_zone.max.x),
                    Random.Range(wander_zone.min.y, wander_zone.max.y));
        }

        seeker.StartPath(rb.position, t, OnPathCalc);
    }

    void OnPathCalc(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
            reachedEndOfPath = false;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.fixedDeltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (force.x >= 0.01)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (force.x <= 0.01)
            transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Dagger d = collision.gameObject.GetComponent<Dagger>();
        if(d)
        {
            d.owner.collectDagger(d.gameObject);
            // take damage
            return;
        }
        Player p = collision.gameObject.GetComponent<Player>();
        if(p)
        {
            // deal damage
            p.hurt();
            //p.GetComponent<Rigidbody2D>().AddForce(collision.GetContact(0).)
        }
    }
}
