using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slow : MonoBehaviour
{
    [SerializeField] float slowFactor = 0.5f;
    [SerializeField] float continuousSlowFactor = 0.01f;

    [SerializeField] ParticleSystem spray;

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (slowFactor == 0)
            return;
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if(rb)
        {
            Vector2 v = rb.velocity;
            v *= slowFactor;
            rb.velocity = v;

            // if Dagger do spraySmall, if player do sprayLarge?
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x);
            Quaternion rot = Quaternion.Euler(angle, 90, 0);
            Instantiate(spray, collision.transform.position, rot);
        }
    }

    // Start is called before the first frame update
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (continuousSlowFactor == 0)
            return;
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb)
        {
            Vector2 v = rb.velocity;
            v *= continuousSlowFactor * Time.fixedDeltaTime;
            rb.velocity = v;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
}
