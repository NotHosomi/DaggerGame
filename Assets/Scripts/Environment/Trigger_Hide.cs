using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Hide : MonoBehaviour
{
    [SerializeField] float fade_time = 0.75f;
    [SerializeField] bool permanent = false;

    SpriteRenderer[] sprites;

    void Start()
    {
        sprites = GetComponents<SpriteRenderer>();
    }

    Coroutine in_co;
    Coroutine out_co;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.GetComponent<Player>())
            return;

        if (in_co != null)
            StopCoroutine(in_co);
        out_co = StartCoroutine(FadeOut());
        if (permanent)
            Destroy(gameObject, fade_time);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!permanent && collision.GetComponent<Player>())
        {
            if(out_co != null)
                StopCoroutine(out_co);
            in_co = StartCoroutine(FadeIn());
        }
    }

    float progression = 1;
    IEnumerator FadeOut()
    {
        while(true)
        {
            progression -= Time.deltaTime / fade_time;
            if (progression < 0)
                progression = 0;

            float a = Mathf.Lerp(0, 1, progression);
            foreach (SpriteRenderer s in sprites)
            {
                Color c = s.material.color;
                c.a = a;
                s.material.color = c;
            }
            if (progression <= 0)
                break;
            yield return null;
        }
    }
    IEnumerator FadeIn()
    {
        while (true)
        {
            progression += Time.deltaTime / fade_time;
            if (progression > 1)
                progression = 1;

            float a = Mathf.Lerp(0, 1, progression);
            foreach (SpriteRenderer s in sprites)
            {
                Color c = s.material.color;
                c.a = a;
                s.material.color = c;
            }
            if (progression >= 1)
                break;
            yield return null;
        }
    }
}
