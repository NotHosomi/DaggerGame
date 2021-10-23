using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DgrGravity : Dagger
{
    [SerializeField] float grav_mult;
    float defaultgrav;
    bool doubletapped = false;

    private void Start()
    {
        defaultgrav = rb.gravityScale;
    }

    public override void alt()
    {
        if(in_air)
        {
            rb.gravityScale *= grav_mult;
            if (doubletapped == true)
                fizzleDagger();
            doubletapped = true;
            return;
        }
        doubletapped = false;

        Vector2 vel = rb.velocity;
        float temp = 0;
        if(vel.y < 0)
        {
            temp = vel.y;
        }
        vel.x = 0;
        vel.y = 0;

        bool grounded = false;
        Vector2 n = getHitNorm();
        n.y /= 2;
        n.Normalize();
        grounded = (n.y > Mathf.Abs(n.x)) && !in_air;
        if (grounded)
        {
            owner.collectDagger();
            return;
        }

        //transform.rotation = Quaternion.Euler(0, 0, 270);
        
        //Vector2 size = GetComponent<BoxCollider2D>().size;
        //float temp = size.x;
        //size.x = size.y;
        //size.y = temp;
        //size.y /= 2;
        //
        //Physics2D.BoxCast



        rb.constraints = RigidbodyConstraints2D.None;
    }
}
