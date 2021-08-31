using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoDagger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Dagger d = other.GetComponent<Dagger>();
        if (d)
        {
            d.fizzleDagger();
        }
    }
}
