using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedSwitch : ActionableGeneric
{
    public override void fireInput(int i)
    {
        switch (i)
        {
            case 0: // OnDaggerHit
                if (inverted)
                    turnOn();
                else
                    turnOff();
                break;
            case 1: // Reset
                if (inverted)
                    turnOff();
                else
                    turnOn();
                break;
        }
    }

    [SerializeField] bool inverted;
    bool active;
    float timer;
    [SerializeField] float timerDuration;
    InteractableGeneric IG;
    SpriteRenderer SR;


    [SerializeField] Sprite on_sprite;
    [SerializeField] Sprite off_sprite;

    private void Start()
    {
        IG = GetComponent<InteractableGeneric>();
        SR = GetComponent<SpriteRenderer>();
        if (inverted)
        {
            SR.color = Color.grey;
            SR.sprite = off_sprite;
        }
        else
        {
            SR.color = Color.yellow;
            SR.sprite = on_sprite;
        }
        active = !inverted;
    }

    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                timer = 0;
                if(inverted)
                {
                    // turning off
                    turnOff();
                }
                else
                {
                    turnOn();
                }
            }
        }
    }

    // Start is called before the first frame update
    void turnOn()
    {
        if (active)
        {
            Debug.Log("Torch is already on");
            return; // already on
        }

        active = true;
        if (inverted)
        {
            timer = timerDuration;
            SR.color = Color.cyan;
        }
        else
        {
            timer = 0;
            SR.color = Color.yellow;
        }
        SR.sprite = on_sprite;
        IG.FireUser(1);
    }

    // Update is called once per frame
    void turnOff()
    {
        if (!active)
        {
            Debug.Log("Torch is already off");
            return; // already off
        }

        active = false;
        if (!inverted)
        {
            timer = timerDuration;
        }
        else
        {
            timer = 0;
        }
        SR.color = Color.grey;
        SR.sprite = off_sprite;
        IG.FireUser(2);
    }
}
