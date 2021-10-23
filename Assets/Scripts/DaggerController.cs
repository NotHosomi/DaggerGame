using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerController : MonoBehaviour
{
    enum DaggerType
    {
        Tutorial,   // No blink, must be touched to be collected
        Default,    // Right click deletes the dagger
        Gravity,    // M2 drops the dagger from it's stuck position. Collects if on ground.
        Homing,     // Returns in a straight line to player (or accelerates towards player?)
        Double,     // Two blinks, no return ability, need to quickswitch to abort.
        Grapple,    // No drop, M1down extends, M1up retracts, M2held pulls player IF landed
        DoubleGrapple, // Same as above. M1up pulls the player if landed. M1down whilst retracting releases. M1 mirrored for Dagger2 on M2
        Heavy,      // Doesn't float in water
        //Time,     // Player can hold blink button to move blink pos back through the trajectory
        Lingering,  // Isn't deleted on dagger switch
        Length      // UTIL
    };
    DaggerType dagger_type = DaggerType.Default;

    [SerializeField] LayerMask LM;
    [SerializeField] float dgr_throw_coeff;
    [SerializeField] float dgr_weight;
    [SerializeField] float dgr_charge_time;
    [SerializeField] float dgr_min_charge;
    [SerializeField] AnimationCurve dgr_charge_curve;
    [SerializeField] GameObject[] dagger_prefabs;

    GameObject[] dagger = { null, null, null };
    Transform[] charge_bars = { null, null, null };
    float[] charge = { 0, 0 };

    // hint line
    LineRenderer[] hint_lines = { null, null, null };
    int hint_r_limit = 20;
    float hint_timescale = 0.1f;
    float hint_rescale = 0.515f; // approximate gravity alignment correction. No fuckin clue why tho

    private void Awake()
    {
        int i = 0;
        int j = 0;
        LineRenderer LR;
        foreach (Transform child in transform)
        {
            if (child.tag == "PLAYER_bars")
            {
                charge_bars[i] = child;
                ++i;
            }
            if (child.tag == "PLAYER_arcs")
            {
                if (LR = child.GetComponent<LineRenderer>())
                {
                    hint_lines[j] = LR;
                    ++j;
                }
                else
                {
                    Debug.LogError("PlayerChild tagged ARC is missing Linerenderer");
                }
            }
        }
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



        switch (dagger_type)
        {
            case DaggerType.Default:
            case DaggerType.Gravity:
            case DaggerType.Homing:
            case DaggerType.Heavy:
                ControlDefault();
                break;
            case DaggerType.Tutorial:
                ControlTutorial();
                break;
            case DaggerType.Double:
                break;
            case DaggerType.Grapple:
                break;
            case DaggerType.DoubleGrapple:
                break;
            case DaggerType.Lingering:
                break;
            default:
                Debug.Log("Dagger behavior unlinked");
                break;
        }

        for(int i = 0; i < (int)DaggerType.Length; ++i)
            if (Input.GetKeyDown((KeyCode)i + 48))
            {
                dagger_type = (DaggerType)i;
                Debug.Log("Dagger type: " + dagger_type);
            }
    }

    // Dagger controls
    void ControlDefault()
    {
        if (dagger[0] == null)
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
                dagger[0].GetComponent<Dagger>().alt();
            }
        }
    }

    // Dagger controls
    void ControlTutorial()
    {
        if (dagger[0] == null)
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
    }

    // Dagger controls
    void ControlDouble()
    {
        if (dagger[1] == null)
        {
            if (Input.GetMouseButton(0))
            {
                chargeThrow(1);
            }
            if (Input.GetMouseButtonUp(0))
            {
                throwDagger(1);
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                blink();
            }
        }
    }





    void chargeThrow(int id = 0)
    {
        charge[id] += Time.deltaTime / dgr_charge_time;
        charge[id] = Mathf.Min(charge[id], 1);

        Vector3 s = charge_bars[id].localScale;
        s.x = charge[id];
        charge_bars[id].localScale = s;

        Vector2 dir = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        dir.Normalize();
        renderArc(dir * dgr_charge_curve.Evaluate(charge[id]) * dgr_throw_coeff, id);

    }

    void throwDagger(int id = 0)
    {
        //if (charge < dgr_min_charge)
        //    charge = dgr_min_charge;
        dagger[id] = Instantiate(fetchDaggerPrefab(id), transform.position, new Quaternion());
        Vector2 dir = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - transform.position;
        dir.Normalize();
        dagger[id].GetComponent<Rigidbody2D>().velocity = dir * dgr_charge_curve.Evaluate(charge[id]) * dgr_throw_coeff;
        if (dir.x < 0)
            dagger[id].GetComponent<SpriteRenderer>().flipY = true;
        dagger[id].GetComponent<Dagger>().owner = this;
        dagger[id].GetComponent<Rigidbody2D>().gravityScale = dgr_weight;

        // wipe charge
        charge[id] = 0;
        Vector3 s = charge_bars[id].localScale;
        s.x = charge[id];
        charge_bars[id].localScale = s;
        // wipe line renderer
        hint_lines[id].enabled = false;
    }

    GameObject fetchDaggerPrefab(int id)
    {
        switch(dagger_type)
        {
            case DaggerType.Double:
                return dagger_prefabs[(int)DaggerType.Length];
            case DaggerType.DoubleGrapple:
                return dagger_prefabs[(int)DaggerType.Length + 1];
            default:
                return dagger_prefabs[(int)dagger_type];
        }
    }

    void renderArc(Vector2 vel, int id)
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

        hint_lines[id].enabled = true;
        hint_lines[id].positionCount = arc.Length;
        hint_lines[id].SetPositions(arc);
        hint_lines[id].positionCount = (t < arc.Length) ? t + 1 : arc.Length;
    }

    public void forgetDagger(int id = 0)
    {
        dagger[id] = null;
    }

    public void collectDagger(int id = 0)
    {
        Destroy(dagger[id]);
        dagger[id] = null;
    }

    void blink(int d_id = 0)
    {
        #region spatial_checks
        Vector3 hitnorm;
        Vector3 hitloc;
        Vector2 assumption = new Vector2();
        Dagger dgr = dagger[d_id].GetComponent<Dagger>();
        //List<Vector2> contacts = dgr.getHitPosList();

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
        bool clear = false;
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
            if(!clear)
            {
                clear = checkYSpace(ref pos, min_space);
                if(clear)
                {
                    clear = checkXSpace(ref pos, min_space);
                }
            }
        }
        Debug.DrawLine(dgr.transform.position + hitloc, pos, Color.green, 5);

        if (clear)
        {
            transform.position = pos;
            GetComponent<Rigidbody2D>().velocity = dgr.GetComponent<Rigidbody2D>().velocity;
            collectDagger();
        }
        else
        {
            dgr.fizzleDagger();
        }
        #endregion

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

}
