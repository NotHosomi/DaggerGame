using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Progression : MonoBehaviour
{
    public static Progression inst = null;
    private void Awake()
    {
        if (inst == null)
        {
            inst = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (inst != this)
        {
            Destroy(this.gameObject);
            return;
        }
    }


    [Flags] public enum Upgrades
    {
        none,
        double_jump,
        dash,
        dash2,
    }

    Upgrades upgrades = Upgrades.none;
    public void set(Upgrades upgrade)
    {
        upgrades |= upgrade;
        save();
    }
    public bool get(Upgrades upgrade)
    {
        return (upgrades & upgrade) == Upgrades.none;
    }


    void load()
    {
        // todo
    }
    void save()
    {
        // todo
    }
}
