using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] string target_level;
    [SerializeField] int target_level_id;
    [SerializeField] string target_door;

    public enum dir
    {
        none,
        left,
        up,
        right,
        down
    }
    public dir exit_dir;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Hit scene transition");
            if(target_level == "")
                GameManager.gm.LoadLevel(target_level_id, target_door);
            else
                GameManager.gm.LoadLevel(target_level, target_door);
            // TODO: move player in exit_dir
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
