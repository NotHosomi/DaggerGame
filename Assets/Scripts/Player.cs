using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [SerializeField] LayerMask LM;

    // Camera
    //[SerializeField] AnimationCurve cam_pull;
    //[SerializeField] float cam_offset_scale;
    Transform cam;
    Vector2 cam_vel = new Vector2();
    Vector3 cam_offset;
    Vector3 cam_disjoint;
    const float cam_vert_offset = 2;
    public bool cam_locked = false;
    Bounds cam_bounds;

    // Movement
    Rigidbody2D rb;
    [SerializeField] float mv_speed;
    //[SerializeField] float mv_accel;
    //[SerializeField] float mv_airaccel;
    [SerializeField] float mv_friction;
    [SerializeField] float mv_gravity;
    [SerializeField] float mv_hi_jump_grav;
    //[SerializeField] float mv_lo_jump_grav;
    [SerializeField] float mv_maxfall;
    [SerializeField] float mv_jumpforce;
    //[SerializeField] float cam_speed;
    public bool jumping = false;
    Vector2 reset_pos;

    bool has_control = true;
    public float grav_coeff = 1;

    [SerializeField] bool grounded;

    // HP
    public List<GameObject> hp_icons;
    public int hp;

    private void Awake()
    {
        reset_pos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main.transform;
        Vector3 cam_pos = transform.position;
        cam_pos.z = cam.position.z;
        cam_offset.z = cam_pos.z;
        cam_offset.y = cam_vert_offset;
        cam.position = cam_pos;
        hp = GameManager.gm.current_hp;

        cam_bounds = GameObject.Find("Tilemap").GetComponent<Tilemap>().localBounds;
        cam_bounds.max -= new Vector3(19, 11, 0);
        cam_bounds.min += new Vector3(19, 11, 0);
    }

    // smooth cam_offset
    bool facing_right = true;
    private void Update()
    {
        shiftCamOffset();
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
        grounded = Physics2D.OverlapBox(transform.position + Vector3.down, new Vector2(0.78f, 0.1f), 0, LM); // check grounded
        applyGravity();
        if (!has_control)
        {
            return;
        }

        float wish_dir = 0;
        bool wish_jump = false;
        if (Input.GetKey(KeyCode.Space)) // this is to be updated
            wish_jump = true;
        else
            jumping = false;
        if (Input.GetKey(KeyCode.A))
            --wish_dir;
        if (Input.GetKey(KeyCode.D))
            ++wish_dir;

        Vector2 vel = rb.velocity;


        if (Mathf.Abs(vel.x) <= mv_speed + 0.01f) // less than max speed? (Epsilon accounts for floating point err)
            vel.x = wish_dir * mv_speed;
        else if (grounded)
            vel.x *= 0.9f;
        else if (Mathf.Sign(vel.x) != Math.Sign(wish_dir))
            vel.x *= 0.99f;
        //    vel.x -= Mathf.Log(Mathf.Abs(vel.x))/vel.x;   //Debug.Log("TODO: air friction");
        // air movement feels jank sometimes :\


        if (wish_jump && grounded && !jumping) // this is to be updated
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
            /*
            if (vel.y > 0)
            {
                if(jumping)
                {
                    //vel.y -= mv_gravity * mv_hi_jumpgrav_mult * grav_coeff;
                    vel.y -= mv_hi_jump_grav * grav_coeff;
                    if (Input.GetKeyUp(KeyCode.Space))
                        vel.y = 0;
                }
                else
                    //vel.y -= mv_gravity * mv_lo_jumpgrav_mult * grav_coeff;
                    vel.y -= mv_lo_jump_grav * grav_coeff;
            }
            else  // standard falling
                vel.y -= mv_gravity * grav_coeff;
            */
            if (jumping)
            {
                //vel.y -= mv_gravity * mv_hi_jumpgrav_mult * grav_coeff;
                vel.y -= mv_hi_jump_grav * grav_coeff;
                if (Input.GetKeyUp(KeyCode.Space))
                    vel.y = 0;
            }
            else  // standard falling
                vel.y -= mv_gravity * grav_coeff;
        }
        else
            vel.y = mv_maxfall;
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
        if (cam_locked)
            return;

        cam_disjoint.x = Mathf.SmoothDamp(cam_disjoint.x, 0, ref cam_vel.x, 0.25f);
        cam_disjoint.y = Mathf.SmoothDamp(cam_disjoint.y, 0, ref cam_vel.y, 0.25f);
        Vector3 cam_target = transform.position + cam_offset + cam_disjoint;
        cam.position = cam_target;
        camClamp();
    }

    float cam_offset_vel = 0;
    void shiftCamOffset()
    {
        if (!has_control)
            return;
        bool L = Input.GetKey(KeyCode.A);
        bool R = Input.GetKey(KeyCode.D);
        if(L ^ R)
        {
            if (L)
            {
                facing_right = false;
            }
            else if (R)
            {
                facing_right = true;
            }
        }
        cam_offset.x = Mathf.SmoothDamp(cam_offset.x, (facing_right ? 1 : -1), ref cam_offset_vel, 0.25f);
    }

    void camClamp()
    {
        Vector3 pos = cam.position;
        pos.x = Mathf.Clamp(pos.x, cam_bounds.min.x, cam_bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, cam_bounds.min.y, cam_bounds.max.y);
        cam.position = pos;
    }

    bool isDying = false;
    // returns true if the player took damage
    public bool hurt(int dmg = 1, int knockback = 0)
    {
        if (invulnerable)
            return false;

        hp -= dmg;
        hp = Math.Max(0, hp);
        hp_icons[hp].GetComponent<Animator>().SetTrigger("hurt");
        if (hp == 0)
        {
            //death
            isDying = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            Destroy(gameObject, 1f);
            // GetComponent<Animator>().SetTrigger("death")
            return true;
        }
        StartCoroutine(onTakeDamage(knockback));
        return true;
    }

    public bool invulnerable = false;

    public void heal()
    {
        if (hp == GameManager.gm.max_hp)
            return;
        hp_icons[hp].GetComponent<Animator>().SetTrigger("heal");
        hp += 1;
    }

    /*
    *  INTERFACE 
    */
    public void hitHazard()
    {
        Debug.Log("HAZARD");
        rb.velocity *= 0;
        if (isDying)
            return;

        // todo, put this in a coroutine with an animation
        transform.position = reset_pos;
        //move camera
        cam.position = reset_pos;
        cam_disjoint *= 0;
        camClamp();
    }

    public void setRespawn(Vector2 pos)
    {
        Debug.Log("Checkpoint");
        reset_pos = pos;
    }

    Vector2 cam_root_old;
    public void onBlinkEarly()
    {
        //cam_root_old = transform.position;
        //cam_root_old.y -= cam_vert_offset;
        //cam_root_old.x = Mathf.Clamp(cam_root_old.x, cam_bounds.min.x, cam_bounds.max.x);
        //cam_root_old.y = Mathf.Clamp(cam_root_old.y, cam_bounds.min.y, cam_bounds.max.y);
    }
    public void onBlinkLate(Vector3 travelled)
    {
        //cam_disjoint += (Vector3)cam_root_old - transform.position;

        cam_disjoint -= travelled;
        Vector3 cam_target = transform.position + cam_offset + cam_disjoint;
        cam_target.x = Mathf.Clamp(cam_target.x, cam_bounds.min.x, cam_bounds.max.x);
        cam_target.y = Mathf.Clamp(cam_target.y, cam_bounds.min.y, cam_bounds.max.y);
        cam_disjoint = cam_target - transform.position - cam_offset;
    }

    private void OnDestroy()
    {
        if(hp==0)
            GameManager.gm.onPlayerDeath();
    }
    
    IEnumerator onTakeDamage(int dir)
    {
        Time.timeScale = 0.01f;
        GameObject dmg_sprite = transform.GetChild(4).gameObject;
        has_control = false;

        dmg_sprite.SetActive(true);
        dmg_sprite.transform.localScale = new Vector3(-dir * 2.5f, 1, 1);

        invulnerable = true;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);

        yield return new WaitForSecondsRealtime(0.15f);
        dmg_sprite.SetActive(false);
        yield return new WaitForSecondsRealtime(0.15f);

        Time.timeScale = 1f;
        rb.velocity = new Vector3((facing_right ? -10 : 10), 8, 0);

        yield return new WaitForSecondsRealtime(0.2f);
        has_control = true;

        yield return new WaitForSecondsRealtime(0.8f);
        GetComponent<SpriteRenderer>().color = Color.white;
        invulnerable = false;
    }
}