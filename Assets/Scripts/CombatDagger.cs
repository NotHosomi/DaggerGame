using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDagger : MonoBehaviour
{
    protected Rigidbody2D rb;

    //bool hit;
    protected bool in_air = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        Destroy(gameObject, 20);
    }

    private void Update()
    {
        if (rb.velocity.sqrMagnitude > 0)
        {
            Vector2 vel = rb.velocity;
            transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
