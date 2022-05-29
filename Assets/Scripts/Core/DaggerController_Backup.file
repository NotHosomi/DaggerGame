using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerController : MonoBehaviour
{
    [System.Flags]
    enum Upgrades
    {
        Default = 1,
        Heavy = 1 << 1,
        Homing = 1 << 2
    };
    [SerializeField] Upgrades upgrades; // Dagger upgrades specifically. Does not count

    [SerializeField] LayerMask LM;
    [SerializeField] float dgr_throw_coeff;
    [SerializeField] float dgr_weight;
    [SerializeField] float dgr_charge_time;
    [SerializeField] float dgr_min_charge;
    [SerializeField] float dgr_charge_scale;
    [SerializeField] float dgr_cursor_radius;
    [SerializeField] AnimationCurve dgr_charge_curve;
    [SerializeField] GameObject[] dagger_prefabs;

    [SerializeField] int max_daggers; // for now lets just say it maxes out at 3 (+1 for the heavy)


    List<GameObject> dagger = new List<GameObject>();
    GameObject heavy_dagger = null;
    float cooldown = 0;
    [SerializeField] float DGR_COOLDOWN;
    float blink_cd = 0;
    [SerializeField] float BLINK_COOLDOWN;
    [SerializeField] float BLINK_INHERIT_VELOCITY_MULT;


    float charge = 0;
    Transform charge_bar = null;

    // hint line
    LineRenderer hint_line = null;
    int hint_r_limit = 20;
    float hint_timescale = 0.1f;
    float hint_rescale = 0.515f; // approximate gravity alignment correction. No fuckin clue why tho
    Player player;

    private void Awake()
    {
        LineRenderer LR;
        foreach (Transform child in transform)
        {
            if (child.tag == "PLAYER_bars")
            {
                charge_bar = child;
            }
            else if (child.tag == "PLAYER_arcs")
            {
                if (LR = child.GetComponent<LineRenderer>())
                {
                    hint_line = LR;
                }
                else
                {
                    Debug.LogError("PlayerChild tagged ARC is missing Linerenderer");
                }
            }
        }
        player = gameObject.GetComponent<Player>();
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

        ControlDefault();
        ControlHeavy();
        GameObject nearest = findBlinkTarget();
        if(nearest != null)
        {
            foreach(GameObject d in dagger)
            {
                if (d != nearest)
                    d.GetComponent<Animator>().SetBool("Nearest", false);
            }
            if(heavy_dagger != null && heavy_dagger != nearest)
                heavy_dagger.GetComponent<Animator>().SetBool("Nearest", false);
            nearest.GetComponent<Animator>().SetBool("Nearest", true);
            if (Input.GetMouseButtonDown(1) && blink_cd <= 0)
            {
                blink(nearest.GetComponent<Dagger>());
            }
        }
        else
        {
            foreach (GameObject d in dagger)
            {
                d.GetComponent<Animator>().SetBool("Nearest", false);
            }
            if (heavy_dagger != null)
                heavy_dagger.GetComponent<Animator>().SetBool("Nearest", false);
        }

        if (blink_cd > 0)
            blink_cd -= Time.deltaTime;
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
    }

    // Dagger controls
    void ControlDefault()
    {
        if ((upgrades & Upgrades.Default) == 0)
            return;

        if (Input.GetMouseButtonDown(0) && cooldown <= 0 && charge <= 0)
        {
            if (dagger.Count == max_daggers)
            {
                collectDagger(0);
            }
            throwDagger();
        }
    }

    void ControlHeavy()
    {
        if ((upgrades & Upgrades.Heavy) == 0)
            return;

        if (heavy_dagger == null)
        {
            if (Input.GetKey(KeyCode.Q))
                chargeThrow();
            if (Input.GetKeyUp(KeyCode.Q))
                throwHeavyDagger();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
                if ((upgrades & Upgrades.Homing) != 0)
                    heavy_dagger.GetComponent<Dagger>().recall();
                else
                    heavy_dagger.GetComponent<Dagger>().fizzleDagger();
        }
    }

    void throwDagger()
    {
        GameObject dgr = Instantiate(dagger_prefabs[0], transform.position, new Quaternion());
        Vector2 dir = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        dir.Normalize();
        dgr.GetComponent<Rigidbody2D>().velocity = dir * dgr_throw_coeff;
        if (dir.x < 0)
            dgr.GetComponent<SpriteRenderer>().flipY = true;
        dgr.GetComponent<Dagger>().owner = this;
        dgr.GetComponent<Rigidbody2D>().gravityScale = dgr_weight;
        dagger.Add(dgr);
        // begin cooldown
        cooldown = DGR_COOLDOWN;
    }

    void chargeThrow()
    {
        float mdist = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position).magnitude; // could use SqrMag
        charge = mdist / dgr_charge_scale;
        // change this to be proportional from player
        //charge += Time.deltaTime / dgr_charge_time;
        //charge = Mathf.Min(charge, 1);
        //Debug.Log("Charging: " + charge);

        Vector3 s = charge_bar.localScale;
        s.x = charge;
        charge_bar.localScale = s;

        Vector2 dir = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        dir.Normalize();
        renderArc(dir * dgr_charge_curve.Evaluate(charge) * dgr_throw_coeff);
    }
    void throwHeavyDagger()
    {
        //Debug.Log("Charge: " + charge);
        // TODO: if homing dagger, spawn homing dagger prefab instead
        heavy_dagger = Instantiate(dagger_prefabs[1], transform.position, new Quaternion());
        Vector2 dir = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        dir.Normalize();
        heavy_dagger.GetComponent<Rigidbody2D>().velocity = dir * dgr_charge_curve.Evaluate(charge) * dgr_throw_coeff;
        if (dir.x < 0)
            heavy_dagger.GetComponent<SpriteRenderer>().flipY = true;
        heavy_dagger.GetComponent<Dagger>().owner = this;
        heavy_dagger.GetComponent<Rigidbody2D>().gravityScale = dgr_weight;

        // wipe charge
        charge = 0;
        Vector3 s = charge_bar.localScale;
        s.x = charge;
        charge_bar.localScale = s;
        // wipe line renderer
        hint_line.enabled = false;
    }

    // UTILS
    void renderArc(Vector2 vel)
    {
        Vector3[] arc = new Vector3[hint_r_limit];
        arc[0] = transform.position;
        int t = 1;
        for (; t < hint_r_limit; ++t)
        {
            float timestep = t * hint_timescale;
            float x = vel.x * timestep;
            float y = vel.y * timestep - 0.5f * timestep * timestep * dgr_weight * -Physics2D.gravity.y * hint_rescale;
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
        hint_line.positionCount = (t < arc.Length) ? t + 1 : arc.Length;
        // todo, set width in editor to 0.3, 0
    }

    public void forgetDagger(int id)
    {
        dagger.RemoveAt(id);
    }
    public void forgetDagger(GameObject dgr)
    {
        dagger.Remove(dgr);
    }

    public void collectDagger(int id)
    {
        Destroy(dagger[id]);
        dagger.RemoveAt(id);
    }
    public void collectDagger(GameObject dgr)
    {
        dagger.Remove(dgr);
        Destroy(dgr);
    }

    void blink(Dagger dgr)
    {
        if (blink_cd > 0)
            return;

        Vector3 hitnorm;
        Vector3 hitloc;
        Vector2 assumption = new Vector2();

        if (dgr.isLanded())
        {
            hitnorm = dgr.getHitNorm();
            hitloc = dgr.getHitPos();
            assumption.x = 0.5f * Mathf.Sign(hitnorm.x);
            assumption.y = 1f * Mathf.Sign(hitnorm.y);
        }
        else
        {
            hitnorm = -dgr.GetComponent<Rigidbody2D>().velocity.normalized;
            hitloc = new Vector2();
        }

        Vector2 pos = dgr.transform.position + hitloc;
        Debug.DrawLine(transform.position, pos, Color.yellow, 5);

        hitnorm.y /= 2;
        hitnorm.Normalize();
        Vector2 min_space = new Vector2(0.9f, 2f);
        bool clear;
        if (Mathf.Abs(hitnorm.y) < Mathf.Abs(hitnorm.x))
        {
            pos.x += assumption.x;
            clear = checkYSpace(ref pos, min_space);
            if (!clear)
            {
                clear = checkXSpace(ref pos, min_space);
                if (clear)
                {
                    clear = checkYSpace(ref pos, min_space);
                }
            }
        }
        else
        {
            pos.y += assumption.y;
            clear = checkXSpace(ref pos, min_space);
            if (!clear)
            {
                clear = checkYSpace(ref pos, min_space);
                if (clear)
                {
                    clear = checkXSpace(ref pos, min_space);
                }
            }
        }
        Debug.DrawLine(dgr.transform.position + hitloc, pos, Color.green, 5);

        if (clear)
        {
            player.onBlinkEarly();

            blink_cd = BLINK_COOLDOWN;
            cooldown = DGR_COOLDOWN;
            Vector2 travelled = pos - (Vector2)transform.position; 
            transform.position = pos;
            GetComponent<Rigidbody2D>().velocity += dgr.GetComponent<Rigidbody2D>().velocity * BLINK_INHERIT_VELOCITY_MULT;
            collectDagger(dgr.gameObject);
            GetComponent<Player>().jumping = false;

            player.onBlinkLate(travelled);
        }
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
            dist = min_space.y / 2 - diff;
            pos.y -= dist;
        }
        else if (down_hit)
        {
            diff = -(down_hit.point - pos).y;
            dist = min_space.y / 2 - diff;
            pos.y += dist;
        }
        return true;
    }

    // This doesn't seem to scale correctly with the parameter properly, but whatever
    GameObject findBlinkTarget()
    {
        GameObject dgr = null;
        float lowest_diff = dgr_cursor_radius;
        Vector2 cursorpos = Input.mousePosition; 
        float diff;
        if (dagger.Count != 0)
        {
            foreach (GameObject d in dagger)
            {
                diff = ((cursorpos - (Vector2)Camera.main.WorldToScreenPoint(d.transform.position)) / Screen.height).sqrMagnitude;
                if (lowest_diff > diff)
                {
                    lowest_diff = diff;
                    dgr = d;
                }
            }
        }
        // and check the heavy dagger
        if (heavy_dagger != null && !heavy_dagger.GetComponent<Dagger>().isReturning() 
                && lowest_diff > ((cursorpos - (Vector2)Camera.main.WorldToScreenPoint(heavy_dagger.transform.position)) / Screen.height).sqrMagnitude)
            return heavy_dagger;
        return dgr;
    }
}
