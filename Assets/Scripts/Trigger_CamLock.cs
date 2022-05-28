using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_CamLock : MonoBehaviour
{
    [SerializeField] bool lock_x;
    [SerializeField] bool lock_y;
    [SerializeField] Vector2 lock_pos;
    [SerializeField] bool release_x;
    [SerializeField] bool release_y;
    CamControl cc;

    private void Start()
    {
        cc = GameManager.gm.player.GetComponent<CamControl>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponent<Player>())
            return;
        cc.lockTo((Vector2)transform.position + lock_pos, lock_x, lock_y);  // implement a Lock priority system?
        // blend between positions, not just between locked/not locked
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.GetComponent<Player>())
            return;
        cc.release(release_x, release_y);
    }

    private void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        Gizmos.DrawWireCube((Vector2)transform.position + lock_pos, new Vector2(height * cam.aspect, height));
    }
}
