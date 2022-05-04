using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeBot : MonoBehaviour
{
    Transform player;
    [SerializeField] Animator anim;
    [SerializeField] GameObject projectile;
    [SerializeField] GameObject projectile2;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //anim = GetComponent<Animator>();
    }

    float cooldown = 0;
    private void Update()
    {
        if(cooldown <= 0)
        {
            PickBehaviour();

            //cooldown = 5f;
            //StartCoroutine(spin(3, 180 + transform.rotation.eulerAngles.z));
        }
        cooldown -= Time.deltaTime;
    }

    void PickBehaviour()
    {
        int b = Random.Range(0, 4);
        switch (b)
        {
            case 0:
            case 1:
            case 2:
                projectileAtk();
                break;
            case 3:
                cooldown = 5f;
                StartCoroutine(attackDown());
                break;

        }
        Debug.Log("Pick attack, cd: " + cooldown);
    }

    void projectileAtk()
    {
        if(player.position.y - 3 > transform.position.y)
        {
            StartCoroutine(attackUp(player.position.x < transform.position.x));
        }
        else
        {
            cooldown = 6.33f;
            StartCoroutine(attackDown());
        }
    }



    IEnumerator attackUp(bool left)
    {
        if(left)
            anim.SetTrigger("atk_up_l");
        else
            anim.SetTrigger("atk_up_r");

        anim.SetTrigger("atk_up_r");
        cooldown = 3.25f;
        for (int i = 0; i < 4; ++i)
        {
            Vector2 dir = (Vector2)player.position - (Vector2)transform.position;

            yield return new WaitForSeconds(0.25f);
            GameObject dgr = Instantiate(projectile);
            dgr.transform.position = transform.GetChild(0).transform.position;

            float rot_z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rot_z += Random.Range(-5f, 5f);
            dgr.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);

            dgr.GetComponent<Rigidbody2D>().velocity = dgr.transform.up * 30f;
            Destroy(dgr, 10);
        }
    }

    IEnumerator attackDown()
    {
        Debug.Log("Down attack");
        anim.SetTrigger("atk_down");
        // fade in: 0.5
        // jump: 0.5
        // spin: 0.5
        // hold: 0.7166
        // shoot: 0.7
        // unspin: 0.0833
        // land: 1.5

        yield return new WaitForSeconds(1);
        // TODO spin
        // find desired rot, set angular velocity
        Vector2 dir = player.transform.position - transform.position;
        float theta = Mathf.Atan2(dir.y, dir.x) * 180/Mathf.PI;
        
        StartCoroutine(spin(0.5f, theta));
        yield return new WaitForSeconds(0.5f); // spin
        // end spin
        yield return new WaitForSeconds(0.7166f); // delay

        // shoot
        Vector3 pos = transform.GetChild(0).position - transform.GetChild(0).up * 0.5f;
        dir = player.transform.position - pos;
        dir.Normalize();
        Debug.DrawLine(pos, (Vector2)pos + dir, Color.blue, 5);

        int shard_count = 32;
        for (int i = 0; i < shard_count; ++i)
        {
            Vector2 lcl_dir = dir += new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f));
            lcl_dir.Normalize();
            GameObject shard = Instantiate(projectile2, pos, transform.rotation);
            //shard.GetComponent<Rigidbody2D>().velocity = -shard.transform.up * 50f;
            shard.GetComponent<Rigidbody2D>().velocity = lcl_dir * 50f;
            yield return new WaitForSeconds(0.6f / shard_count);
        }

        Debug.Log("Call: " + 360);
        StartCoroutine(spin(0.833f, 360));
    }

    IEnumerator spin(float duration, float end)
    {
        float start = transform.rotation.eulerAngles.z;
        Debug.Log("spin from " + start + " to " + end);

        if (start == end)
            yield break;
        float t0 = Time.time;
        float smooth;
        float prev = 0;
        float t = 0;
        Transform pivot = transform.GetChild(0);
        Debug.DrawLine(new Vector3(0, 0, 0), transform.position, Color.green, 3);
        Debug.DrawLine(transform.position, transform.GetChild(0).position, Color.green, 3);
        while (t < duration)
        {
            t = (Time.time - t0) / duration;
            smooth = Mathf.SmoothStep(start, end, t);
            foreach(Transform child in transform)
            {
                transform.RotateAround(pivot.position, Vector3.forward, smooth - prev);
            }

            prev = smooth;
            yield return 0; // wait a frame
        }
        foreach (Transform child in transform)
        {
            transform.rotation = Quaternion.Euler(0, 0, end);
        }
        Debug.Log("end spin: " + transform.rotation.eulerAngles.z);
    }
}
