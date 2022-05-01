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
            //PickBehaviour();
            StartCoroutine("attackUp");
        }
        cooldown -= Time.deltaTime;
    }

    void PickBehaviour()
    {
        int b = Random.Range(0, 1);
        switch (b)
        {
            case 0:
            case 1:
            case 2:
                projectileAtk();
                break;
            case 3:
                cooldown = 5f;
                break;

        }
    }

    void projectileAtk()
    {
        if(player.position.y < transform.position.y)
        {
            // downwards attack
            if (player.position.x < transform.position.x)
            {
                anim.SetTrigger("atk_up_l");
                StartCoroutine(attackUp());
            }
            else
            {
                anim.SetTrigger("atk_up_r");
                StartCoroutine(attackUp());
            }
        }
        else
        {
            cooldown = 2f;
            // upwards attack
            if (player.position.x < transform.position.x)
            {

            }
            else
            {

            }
        }
    }



    IEnumerator attackUp()
    {
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

            dgr.GetComponent<Rigidbody2D>().velocity = dgr.transform.up * 15;
            Destroy(dgr, 10);
        }
    }

    IEnumerator attackDown()
    {
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < 16; ++i)
        {
            yield return new WaitForSeconds(0.05f);
            GameObject dgr = Instantiate(projectile2);
        }
    }
}
