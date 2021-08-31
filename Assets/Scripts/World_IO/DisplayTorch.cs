using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTorch : ActionableGeneric
{
    public override void fireInput(int i)
    {
        count += i;
        refresh();
    }

    SpriteRenderer SR;
    [SerializeField] Sprite on_sprite;
    [SerializeField] Sprite off_sprite;

    private void Start()
    {
        SR = GetComponent<SpriteRenderer>();
        refresh();
    }

    [SerializeField] int req_count;
    int count = 0;
    void refresh()
    {
        Debug.Log("Display refresh: " + count);
        if(count >= req_count)
        {
            SR.color = Color.green;
            SR.sprite = on_sprite;
        }
        else
        {
            SR.color = Color.red;
            SR.sprite = off_sprite;
        }
    }
}
