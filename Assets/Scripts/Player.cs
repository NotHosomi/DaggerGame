using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] LayerMask LM;

    // Camera
    [SerializeField] AnimationCurve cam_pull;
    [SerializeField] float cam_offset_scale;
    Transform cam;
    Vector3 cam_center;
    Vector2 cam_vel = new Vector2();
    [SerializeField] float cam_vert_const;

    // Movement
    Rigidbody2D rb;
    [SerializeField] float mv_speed;
    [SerializeField] float mv_accel;
    [SerializeField] float mv_airaccel;
    [SerializeField] float mv_friction;
    [SerializeField] float mv_gravity;
    [SerializeField] float mv_hi_jump_grav;
    [SerializeField] float mv_lo_jump_grav;
    [SerializeField] float mv_maxfall;
    [SerializeField] float mv_jumpforce;
    [SerializeField] float cam_speed;
    public bool jumping = false;
    Vector2 respawn_pos;

    bool has_control = true;
    public float grav_coeff = 1;

    [SerializeField] bool grounded;

    // HP
    [SerializeField] SpriteRenderer[] hp_icons;
    [SerializeField] int max_hp;
    int hp;

    private void Awake()
    {
        respawn_pos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main.transform;
        cam_center = transform.position;
        cam_center.z = cam.position.z;
        cam.position = cam_center;
        hp = max_hp;
    }
    
    /*
    *  MOVEMENT 
    */
    private void FixedUpdate()
    {
        move();
    }

    private void LateUpdate()
    {
        camMove();
    }

    // Old movement, slippery
    /*
    void move()
    {
        applyGravity();
        if (!has_control)
        {
            return;
        }

        float wish_dir = 0;
        bool wish_jump = false;
        if (Input.GetKey(KeyCode.Space))
        {
            wish_jump = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            --wish_dir;
        }
        if (Input.GetKey(KeyCode.D))
        {
            ++wish_dir;
        }
        
        Vector2 vel = rb.velocity;

        // ignore player input if they're over max speed
            if (Mathf.Abs(vel.x) < mv_speed && wish_dir != 0)
        {
            float accel = wish_dir * (grounded ? mv_accel : mv_airaccel) * Time.fixedDeltaTime;
            vel.x += accel;
            // if hit max speed
            if (Mathf.Abs(vel.x) > mv_speed && Mathf.Sign(vel.x) == Mathf.Sign(wish_dir))
            {
                vel.x = wish_dir * mv_speed;
            }
        }
        else if (grounded)
        {
            applyFriction();
        }

        if (wish_jump && grounded)
        {
            vel.y = mv_jumpforce;
        }

        rb.velocity = vel;
    }
    */

    void move()
    {
        checkGrounded();
        applyGravity();
        if (!has_control)
        {
            return;
        }

        float wish_dir = 0;
        bool wish_jump = false;
        if (Input.GetKey(KeyCode.Space)) // this is to be updated
        {
            wish_jump = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            --wish_dir;
        }
        if (Input.GetKey(KeyCode.D))
        {
            ++wish_dir;
        }

        Vector2 vel = rb.velocity;


        if (Mathf.Abs(vel.x) <= mv_speed + 0.01f) // less than max speed? (Epsilon accounts for floating point err)
            vel.x = wish_dir * mv_speed;
        else if (grounded)
            vel.x *= 0.9f;
        else if (Mathf.Sign(vel.x) != Math.Sign(wish_dir))
            vel.x *= 0.99f;
        //    vel.x -= Mathf.Log(Mathf.Abs(vel.x))/vel.x;   //Debug.Log("TODO: air friction");
        // air movement feels jank sometimes :\


        if (wish_jump && grounded) // this is to be updated
        {
            jumping = true;
            vel.y = mv_jumpforce;
        }
        if(!(wish_jump || grounded))
        {
            jumping = false;
        }

        rb.velocity = vel;
    }
    void checkGrounded()
    {
        grounded = Physics2D.OverlapBox(transform.position + Vector3.down, new Vector2(0.78f, 0.1f), 0, LM);
    }

    void applyGravity()
    {// no delta time used, this is in FixedUpdate
        Vector2 vel = rb.velocity;
        if(grounded)
        {
            vel.y = 0;
            rb.velocity = vel;
            return;
        }

        if (vel.y > mv_maxfall)
        {
            if (vel.y > 0) // standard falling
            {
                if(jumping)
                    //vel.y -= mv_gravity * mv_hi_jumpgrav_mult * grav_coeff;
                    vel.y -= mv_hi_jump_grav * grav_coeff;
                else
                    //vel.y -= mv_gravity * mv_lo_jumpgrav_mult * grav_coeff;
                    vel.y -= mv_lo_jump_grav * grav_coeff;
            }
            else
                vel.y -= mv_gravity * grav_coeff;
        }
        else// if (!grounded)
            vel.y = mv_maxfall;
        //else 
        rb.velocity = vel;
    }

    // Why isn't this working ;w;
    void applyFriction()
    {
        Vector2 vel = rb.velocity;
        
        if (Mathf.Abs(vel.x) < 0.01)
        {
            vel.x = 0;
            rb.velocity = vel;
            return;
        }

        float drop = Mathf.Abs(vel.x) * mv_friction * Time.fixedDeltaTime;
        
        float newspeed = Mathf.Abs(vel.x) - drop;
        if (newspeed < 0)
            newspeed = 0;
        newspeed /= Mathf.Abs(vel.x);

        vel.x = vel.x * newspeed;
        Debug.Log(newspeed);
        rb.velocity = vel;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down, new Vector3(0.78f, 0.1f, 1));
    }

    void camMove()
    {
        cam_center.x = Mathf.SmoothDamp(cam_center.x, transform.position.x, ref cam_vel.x, 0.25f);                  // HK cam DOES NOT smoothstep
        cam_center.y = Mathf.SmoothDamp(cam_center.y, transform.position.y + cam_vert_const, ref cam_vel.y, 0.25f); // It is locked to a point just ahead of the player.
                                                                                                                    // could smoothstep this targetpoint only after a blink

        //Vector2 offset = Input.mousePosition;
        //Vector2 screen_half;
        //screen_half.x = Screen.width / 2;
        //screen_half.y = Screen.height / 2;
        //offset = (offset - screen_half) / screen_half;
        //
        //offset.x = cam_pull.Evaluate(Mathf.Abs(offset.x)) * Mathf.Sign(offset.x);
        //offset.y = cam_pull.Evaluate(Mathf.Abs(offset.y)) * Mathf.Sign(offset.y);
        //offset *= cam_offset_scale;
        //cam.position = cam_center + (Vector3)offset;
        cam.position = cam_center;

        //Vector3 pos = cam.position;
        //pos.x = Mathf.SmoothDamp(pos.x, transform.position.x, ref cam_vel.x, 0.5f);
        //pos.y = Mathf.SmoothDamp(pos.y, transform.position.y, ref cam_vel.y, 0.5f);
        //cam.position = pos;
    }

    /*
    *  INTERFACE 
    */
    public void hitHazard()
    {
        Debug.Log("HAZARD");
        transform.position = respawn_pos;
        rb.velocity *= 0;
    }

    public void setRespawn(Vector2 pos)
    {
        Debug.Log("Checkpoint");
        respawn_pos = pos;
    }
}

// Idea:
// keydown when in air
// player speed massively reduced
// 5 quick attacks
// then speed returns