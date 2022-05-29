using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLinear : MonoBehaviour
{
    [SerializeField] float speed = 1;
    [SerializeField] Vector2 offset = new Vector2();
    [SerializeField] bool isOpen = false;
    bool isOpening = false;
    bool isClosing = false;

    Vector2 pos0;
    Vector2 pos1;
    float progress;

    // Start is called before the first frame update
    void Start()
    {
        pos0 = transform.position;
        pos1 = transform.position + (Vector3)offset;
        if (isOpen)
            transform.position = pos1;
    }

    private void Update()
    {
        if (isOpening)
            opening();
        else if (isClosing)
            closing();
    }

    public void toggle()
    {
        if (isOpen)
            close();
        else
            open();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)offset);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + (Vector3)offset + (Vector3)GetComponent<BoxCollider2D>().offset, GetComponent<BoxCollider2D>().size);
    }

    public void open()
    {
        isOpening = true;
        isClosing = false;
    }

    public void close()
    {
        isOpening = false;
        isClosing = true;
    }

    public void opening()
    {
        progress += Time.deltaTime * speed;
        if(progress > 1)
        {
            progress = 1;
            isOpening = false;
        }
        transform.position = pos0 + offset * progress;
    }

    public void closing()
    {
        progress -= Time.deltaTime * speed;
        if (progress < 0)
        {
            progress = 0;
            isClosing = false;
        }
        transform.position = pos0 + offset * progress;
    }
}
