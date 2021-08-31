using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] float player_modifier;
    [SerializeField] float object_modifier;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name + " entered water");
        Player p = collision.gameObject.GetComponent<Player>();
        Dagger d = collision.gameObject.GetComponent<Dagger>();
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (p)
        {
            p.grav_coeff = player_modifier;
        }
        else if (rb)
        {
            rb.gravityScale *= object_modifier;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name + " exited water");
        Player p = collision.gameObject.GetComponent<Player>();
        Dagger d = collision.gameObject.GetComponent<Dagger>();
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (p)
        {
            p.grav_coeff = 1;
        }
        else if (rb)
        {
            rb.gravityScale /= object_modifier;
        }
    }
}
