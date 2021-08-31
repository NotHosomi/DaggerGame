using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionableGeneric : MonoBehaviour
{
    //public abstract void fireUser1();
    //public virtual void fireUser2() { Debug.Log("") }
    //public virtual void fireUser3() { }
    //public virtual void fireUser4() { }
    public abstract void fireInput(int event_id);
}
