using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InteractableGeneric : MonoBehaviour
{
    enum EventType
    {
        OnTriggerEnter,
        OnTriggerExit,
        OnTriggerStay,
        OnInteractKey,
        FireUser1,
        FireUser2,
        FireUser3,
        FireUser4,
    }
    [System.Serializable]
    class OutputRelationship
    {
        public EventType triggerEvent;
        public LayerMask activator;
        public ActionableGeneric target;
        public int fireOutputNamed;
    }

    [SerializeField] OutputRelationship[] outputs;

    float linger_tmr;
    float linger_tick = 1;
    int lingering = 0;
    private void FixedUpdate()
    {
        if(lingering > 0)
            linger_tmr += Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ++lingering;
        interactCommand = false;
        onEvent(EventType.OnTriggerEnter, collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        --lingering;
        if(lingering < 1)
        {
            linger_tmr = 0;
        }
        onEvent(EventType.OnTriggerExit, collision.gameObject);
        interactCommand = false;
    }

    bool interactCommand = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            interactCommand = true;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {

        if (linger_tmr > linger_tick)
        {
            onEvent(EventType.OnTriggerStay, collision.gameObject);
            linger_tmr = 0;
        }

        if (interactCommand)
        {
            onEvent(EventType.OnInteractKey, collision.gameObject);
            interactCommand = false;
        }
    }

    //private void Fire(int id)
    //{
    //    for(int i = 0; i < 4; ++i)
    //    {
    //        if(((int)output & (1 << i)) != 0) // FUUUUCK C#
    //        {
    //            switch (output)
    //            {
    //                case OutputNamed.fireUser1:
    //                    break;
    //                case OutputNamed.fireUser2:
    //                    break;
    //                case OutputNamed.fireUser3:
    //                    break;
    //                case OutputNamed.fireUser4:
    //                    break;
    //                default: Debug.Log("Invalid output on interactable " + gameObject.name);
    //                    break;
    //            }
    //        }
    //    }
    //}

    //int[] checkInputType(InputType it)
    //{
    //    List<int> indices = new List<int>();
    //    for(int i = 0; i < outputs.Length; ++i)
    //    {
    //        if (outputs[i].onInput == it)
    //            indices.Add(i);
    //    }
    //    return indices.ToArray();
    //}

    void onEvent(EventType e, GameObject activator = null)
    {
        for (int i = 0; i < outputs.Length; ++i)
        {
            if (outputs[i].triggerEvent == e && (activator == null || (outputs[i].activator & (1 << activator.layer)) > 0))
                outputs[i].target.fireInput(outputs[i].fireOutputNamed);
        }
    }

    public void FireUser(int id)
    {
        switch(id)
        {
            case 1:
                onEvent(EventType.FireUser1);
                break;
            case 2:
                onEvent(EventType.FireUser2);
                break;
            case 3:
                onEvent(EventType.FireUser3);
                break;
            case 4:
                onEvent(EventType.FireUser4);
                break;
            default: Debug.LogError("Invalid fireUser call to " + gameObject.name + " (Value: " + id.ToString() + ")");
                break;
        }
    }
}
