using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [SerializeField] LayerMask LM;

    // Movement
    public InputMaster controls;
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
    public bool jumping = false;
    Vector2 reset_pos;
    bool wallhang = false;

    public bool has_control = true;
    public float grav_coeff = 1;

    [SerializeField] bool grounded;

    // HP
    public List<GameObject> hp_icons;
    public int hp;

    private void Awake()
    {
        reset_pos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        hp = GameManager.gm.current_hp;
    }

    /*
    *  MOVEMENT 
    */
    private void FixedUpdate()
    {
        move();
    }

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
        if (wallhang)
            wish_dir = 0;

        if (wish_jump || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A) || !Input.GetMouseButton(1))
        {
            wallhang = false;
            grounded = true; // just for this frame
        }

        Vector2 vel = rb.velocity;
        if (Mathf.Abs(vel.x) <= mv_speed + 0.01f) // less than max speed? (Epsilon accounts for floating point err)
            vel.x = wish_dir * mv_speed;
        else if (grounded)
            vel.x *= 0.9f;
        else if (Mathf.Sign(vel.x) != Math.Sign(wish_dir))
            vel.x *= 0.99f;


        if (wish_jump && (grounded || wallhang) && !jumping) // this is to be updated
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
        if(grounded || wallhang)
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
        GetComponent<CamControl>().jumpCam(reset_pos);
    }

    public void setRespawn(Vector2 pos)
    {
        Debug.Log("Checkpoint");
        reset_pos = pos;
    }

    public void onBlinkEarly()
    {
    }
    public void onBlinkLate(Vector3 travelled)
    {
        GetComponent<CamControl>().setCamDisjoint(travelled);

        if(Input.GetMouseButton(1))
        {
            wallhang = true;
        }
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
        rb.velocity = new Vector3((GetComponent<CamControl>().facing_right ? -10 : 10), 8, 0); // change this

        yield return new WaitForSecondsRealtime(0.2f);
        has_control = true;

        yield return new WaitForSecondsRealtime(0.8f);
        GetComponent<SpriteRenderer>().color = Color.white;
        invulnerable = false;
    }
}