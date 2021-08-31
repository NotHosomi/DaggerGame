using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] LayerMask LM;

    Transform cam;
    Vector2 cam_vel = new Vector2();
    Rigidbody2D rb;
    [SerializeField] float mv_speed;
    [SerializeField] float mv_accel;
    [SerializeField] float mv_airaccel;
    [SerializeField] float mv_friction;
    [SerializeField] float mv_gravity;
    [SerializeField] float mv_jumpforce;
    [SerializeField] float cam_speed;
    Vector2 respawn_pos;

    [SerializeField] bool grounded;

    GameObject dagger_prefab;
    GameObject dagger;
    [SerializeField] float dgr_throw_coeff;
    [SerializeField] float dgr_charge_time;
    [SerializeField] float dgr_min_charge;
    [SerializeField] GameObject charge_bar;
    float charge = 0;

    bool has_control = true;
    public float grav_coeff = 1;

    private void Awake()
    {
        respawn_pos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main.transform;
        dagger_prefab = Resources.Load("Dagger") as GameObject;
        hint_line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            hint_rescale += 0.1f;
            Debug.Log(hint_rescale);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            hint_rescale += 0.01f;
            Debug.Log(hint_rescale);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            hint_rescale -= 0.1f;
            Debug.Log(hint_rescale);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            hint_rescale -= 0.01f;
            Debug.Log(hint_rescale);
        }

        if (dagger == null)
        {
            if (Input.GetMouseButton(0))
            {
                chargeThrow();
            }
            if (Input.GetMouseButtonUp(0))
            {
                throwDagger();
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                blink();
            }
            if (Input.GetMouseButtonUp(1))
            {
                dagger.GetComponent<Dagger>().pullBack();
            }
        }
    }

    private void LateUpdate()
    {
        camMove();
    }

    void chargeThrow()
    {
        charge += Time.deltaTime / dgr_charge_time;
        charge = Mathf.Clamp(charge, dgr_min_charge, 1);

        Vector3 s = charge_bar.transform.localScale;
        s.x = charge;
        charge_bar.transform.localScale = s;

        Vector2 dir = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        dir.Normalize();
        renderArc(dir * charge * charge * dgr_throw_coeff);

    }

    void throwDagger()
    {
        if (charge < dgr_min_charge)
            charge = dgr_min_charge;
        dagger = Instantiate(dagger_prefab, transform.position, new Quaternion());
        Vector2 dir = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        dir.Normalize();
        dagger.GetComponent<Rigidbody2D>().velocity = dir * charge * charge * dgr_throw_coeff;
        if (dir.x < 0)
            dagger.GetComponent<SpriteRenderer>().flipY = true;
        dagger.GetComponent<Dagger>().owner = this;

        // wipe charge
        charge = 0;
        Vector3 s = charge_bar.transform.localScale;
        s.x = charge;
        charge_bar.transform.localScale = s;
        // wipe line renderer
        hint_line.enabled = false;
    }

    public void forgetDagger()
    {
        dagger = null;
    }

    public void collectDagger()
    {
        Destroy(dagger);
        dagger = null;
    }

    void blink()
    {        
        #region spatial_checks
        Vector3 hitnorm;
        Vector3 hitloc;
        Dagger dgr = dagger.GetComponent<Dagger>();
        //List<Vector2> contacts = dgr.getHitPosList();

        if (dgr.isLanded())
        {
            hitnorm = dgr.getHitNorm();
            hitloc = dgr.getHitPos();
        }
        else
        {
            hitnorm = -dagger.GetComponent<Rigidbody2D>().velocity.normalized;
            hitloc = new Vector2();
        }
        
        Vector2 pos = dagger.transform.position + hitloc;
        Debug.DrawLine(transform.position, pos, Color.yellow, 5);

        hitnorm.y /= 2;
        hitnorm.Normalize();
        Vector2 min_space = new Vector2(0.9f, 2f);
        bool clear = false;
        if (Mathf.Abs(hitnorm.y) < Mathf.Abs(hitnorm.x))
        {
            pos.x += Mathf.Sign(hitnorm.x) * 0.5f;
            clear = checkYSpace(ref pos, min_space);
        }
        else
        {
            pos.y += Mathf.Sign(hitnorm.y) * 1f;
            clear = checkXSpace(ref pos, min_space);
        }
        Debug.DrawLine(dagger.transform.position + hitloc, pos, Color.green, 5);

        if(clear)
        {
            transform.position = pos;
        }
        #endregion
        
        rb.velocity = dagger.GetComponent<Rigidbody2D>().velocity;
        collectDagger();
    }

    bool checkXSpace(ref Vector2 pos, Vector2 min_space)
    {
        Vector2 box_size = new Vector2(0.09f, 1.99f);

        Vector2 Temp = pos;
        Temp.x += box_size.x / 2;
        RaycastHit2D right_hit = Physics2D.BoxCast(Temp, box_size, 0, Vector2.right, min_space.x / 2 - box_size.x / 2, LM);
        Debug.DrawRay(pos, Vector2.right * min_space.x / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.right * min_space.x / 2, Vector2.up * box_size.y / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.right * min_space.x / 2, Vector2.down * box_size.y / 2, Color.red, 3);

        Temp = pos;
        Temp.x -= box_size.x / 2;
        RaycastHit2D left_hit = Physics2D.BoxCast(Temp, box_size, 0, Vector2.left, min_space.x / 2 - box_size.x / 2, LM);
        Debug.DrawRay(pos, Vector2.left * min_space.x / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.left * min_space.x / 2, Vector2.up * box_size.y / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.left * min_space.x / 2, Vector2.down * box_size.y / 2, Color.red, 3);


        if (right_hit && left_hit)
        {
            Debug.Log("No room (X)!");
            Debug.Log("Right: " + right_hit.collider.gameObject.name + "\nLeft: " + left_hit.collider.gameObject.name);
            return false;
        }
        float diff = 0;
        float dist = 0;
        if (right_hit)
        {
            diff = (right_hit.point - pos).x;
            dist = min_space.x / 2 - diff;
            pos.x -= dist;
        }
        else if (left_hit)
        {
            diff = -(left_hit.point - pos).x;
            dist = min_space.x / 2 - diff;
            pos.x += dist;
        }
        else
        {
            // no collisions
            return true;
        }
        return true;
    }

    bool checkYSpace(ref Vector2 pos, Vector2 min_space)
    {
        Vector2 box_size = new Vector2(0.8f, 0.19f);

        Vector2 Temp = pos;
        Temp.y += box_size.y / 2;
        RaycastHit2D up_hit = Physics2D.BoxCast(Temp, box_size, 0, Vector2.up, min_space.y / 2 - box_size.y / 2, LM);
        //debug lines
        Debug.DrawRay(pos, Vector2.up * min_space.y / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.up * min_space.y / 2, Vector2.left * box_size.x / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.up * min_space.y / 2, Vector2.right * box_size.x / 2, Color.red, 3);

        Temp = pos;
        Temp.y -= box_size.y / 2;
        RaycastHit2D down_hit = Physics2D.BoxCast(Temp, box_size, 0, Vector2.down, min_space.y / 2 - box_size.y / 2, LM);
        //debug lines
        Debug.DrawRay(pos, Vector2.down * min_space.y / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.down * min_space.y / 2, Vector2.left * box_size.x / 2, Color.red, 3);
        Debug.DrawRay(pos + Vector2.down * min_space.y / 2, Vector2.right * box_size.x / 2, Color.red, 3);

        if (up_hit && down_hit)
        {
            Debug.Log("No room (Y)!");
            Debug.Log("Upper: " + up_hit.collider.gameObject.name + "\nLower: " + up_hit.collider.gameObject.name);
            return false;
        }
        float dist = 0;
        float diff = 0;
        if (up_hit)
        {
            diff = (up_hit.point - pos).y;
            dist = min_space.y/2 - diff;
            pos.y -= dist;
        }
        else if (down_hit)
        {
            diff = -(down_hit.point - pos).y;
            dist = min_space.y/2 - diff;
            pos.y += dist;
        }
        return true;
    }

    // hint line
    LineRenderer hint_line;
    //public float dgr_grav = 2.5f;
    int hint_r_limit = 20;
    float hint_timescale = 0.1f;
    float hint_rescale = 0.515f; // approximate gravity alignment correction. No fuckin clue why tho
    void renderArc(Vector2 vel)
    {

        Vector3[] arc = new Vector3[hint_r_limit];
        arc[0] = transform.position;
        int t = 1;
        for (; t < hint_r_limit; ++t)
        {
            float timestep = t * hint_timescale;
            float x = vel.x * timestep;
            float y = vel.y * timestep - 0.5f * timestep * timestep * dagger_prefab.GetComponent<Rigidbody2D>().gravityScale * -Physics2D.gravity.y * hint_rescale;
            arc[t] = new Vector3(x, y) * 0.5f + transform.position;

            RaycastHit2D hit = Physics2D.Linecast(arc[t - 1], arc[t], LM);
            if (hit)
            {
                arc[t] = hit.point;
                break;
            }
        }

        hint_line.enabled = true;
        hint_line.positionCount = arc.Length;
        hint_line.SetPositions(arc);
        hint_line.positionCount = (t<arc.Length) ? t+1 : arc.Length;
    }

    /*
    *  MOVEMENT 
    */
    private void FixedUpdate()
    {
        checkGrounded();
        move();
    }

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
            Debug.Log("Successful jump");
            vel.y = mv_jumpforce;
        }

        rb.velocity = vel;
    }

    void checkGrounded()
    {
        grounded = Physics2D.OverlapBox(transform.position + Vector3.down, new Vector2(0.79f, 0.1f), 0, LM);
    }

    void applyGravity()
    {
        Vector2 vel = rb.velocity;
        vel.y -= mv_gravity * grav_coeff;
        rb.velocity = vel;
    }

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
        rb.velocity = vel;
    }

    void camMove()
    {
        Vector3 pos = cam.position;
        pos.x = Mathf.SmoothDamp(pos.x, transform.position.x, ref cam_vel.x, 0.5f);
        pos.y = Mathf.SmoothDamp(pos.y, transform.position.y, ref cam_vel.y, 0.5f);
        cam.position = pos;
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