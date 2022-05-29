using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CamControl : MonoBehaviour
{
    Transform cam;
    Vector2 cam_vel = new Vector2();
    Vector3 cam_offset;
    Vector3 cam_disjoint;
    const float cam_vert_offset = 2;
    public bool cam_locked = false;
    Bounds cam_bounds;
    float cam_offset_vel = 0;

    Player p;
    public bool facing_right = true;

    float x_blend = 0;
    float y_blend = 0;
    Vector2 lock_pos;

    private void Awake()
    {
        cam = Camera.main.transform;

        cam_bounds = GameObject.Find("Tilemap").GetComponent<Tilemap>().localBounds;
        cam_bounds.max -= new Vector3(19, 11, 0);
        cam_bounds.min += new Vector3(19, 11, 0);

        Vector3 cam_pos = transform.position;
        cam_pos.z = cam.position.z;
        cam_offset.z = cam_pos.z;
        cam_offset.y = cam_vert_offset;
        cam.position = cam_pos;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        shiftCamOffset();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        camMove();
    }

    void camMove()
    {
        cam_disjoint.x = Mathf.SmoothDamp(cam_disjoint.x, 0, ref cam_vel.x, 0.25f);
        cam_disjoint.y = Mathf.SmoothDamp(cam_disjoint.y, 0, ref cam_vel.y, 0.25f);
        // assemble
        Vector2 cam_target = transform.position + cam_offset + cam_disjoint;
        // clamp
        cam_target.x = Mathf.Clamp(cam_target.x, cam_bounds.min.x, cam_bounds.max.x);
        cam_target.y = Mathf.Clamp(cam_target.y, cam_bounds.min.y, cam_bounds.max.y);
        // blend
        Vector3 pos = cam.position;
        pos.x = Mathf.Lerp(cam_target.x, lock_pos.x, x_blend);
        pos.y = Mathf.Lerp(cam_target.y, lock_pos.y, y_blend);
        cam.position = pos;
        //Debug.Log(pos);
        debugp = pos;
        debugp.z = 0;
    }
    Vector3 debugp;

    void shiftCamOffset()
    {
        //if (!p.has_control)
        //    return;
        bool L = Input.GetKey(KeyCode.A); // To do, input Interface
        bool R = Input.GetKey(KeyCode.D);
        if (L ^ R)
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
        cam_offset.x = Mathf.SmoothDamp(cam_offset.x, (facing_right ? 1 : -1), ref cam_offset_vel, 0.20f);
    }

    public void setCamDisjoint(Vector3 travelled)
    {
        cam_disjoint -= travelled;
        Vector3 cam_target = transform.position + cam_offset + cam_disjoint;
        cam_target.x = Mathf.Clamp(cam_target.x, cam_bounds.min.x, cam_bounds.max.x);
        cam_target.y = Mathf.Clamp(cam_target.y, cam_bounds.min.y, cam_bounds.max.y);
        cam_disjoint = cam_target - transform.position - cam_offset;
    }

    public void jumpCam(Vector3 pos)
    {
        cam_disjoint *= 0;
        pos.x = Mathf.Clamp(pos.x, cam_bounds.min.x, cam_bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, cam_bounds.min.y, cam_bounds.max.y);
        pos.z = cam.position.z;
        cam.position = pos;
    }

    Coroutine bx_up;
    Coroutine bx_down;
    Coroutine by_up;
    Coroutine by_down;
    public void lockTo(Vector2 pos, bool x, bool y)
    {
        if (x)
        {
            lock_pos.x = pos.x;
            if (bx_down != null)
                StopCoroutine(bx_down);
            bx_up = StartCoroutine(blendUpX());
        }
        if (y)
        {
            lock_pos.y = pos.y;
            if (by_down != null)
                StopCoroutine(by_down);
            by_up = StartCoroutine(blendUpY());
        }
    }

    public void release(bool x, bool y)
    {
        if(x)
        {
            if (bx_up != null)
                StopCoroutine(bx_up);
            bx_down = StartCoroutine(blendDownX());
            //cam_disjoint.x = 0;
        }
        if (y)
        {
            if (by_up != null)
                StopCoroutine(by_up);
            by_down = StartCoroutine(blendDownY());
            //cam_disjoint.y = 0;
        }
    }

    IEnumerator blendUpX() // TODO
    {
        while (true)
        {
            x_blend += Time.deltaTime;
            if (x_blend > 1)
            {
                x_blend = 1;
                break;
            }
            yield return null;
        }
    }
    IEnumerator blendDownX() // TODO
    {
        while (true)
        {
            x_blend -= Time.deltaTime * 2;
            if (x_blend < 0)
            {
                x_blend = 0;
                break;
            }
            yield return null;
        }
    }

    IEnumerator blendUpY() // TODO
    {
        while (true)
        {
            y_blend += Time.deltaTime;
            if (y_blend > 1)
            {
                y_blend = 1;
                break;
            }
            yield return null;
        }
    }
    IEnumerator blendDownY() // TODO
    {
        while (true)
        {
            y_blend -= Time.deltaTime * 2;
            if (y_blend < 0)
            {
                y_blend = 0;
                break;
            }
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.gray;
        //Gizmos.DrawSphere(lock_pos, 0.75f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(debugp, 0.25f);
    }

    private void OnDrawGizmos()
    {
    }
}