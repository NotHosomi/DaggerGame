using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject player = null;
    [SerializeField] GameObject player_prefab;


    // scene transition management
    bool respawning = false;
    int respawnScene = 0;
    int cairn_id = 0;


    public static GameManager gm = null;
    // Start is called before the first frame update
    void Awake()
    {
        if (gm == null)
        {
            gm = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (gm != this)
        {
            Destroy(this.gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public void Respawn()
    {
        respawning = true;
        LoadLevel(respawnScene);
    }


    void spawnPlayer(Vector2 position = new Vector2())
    {
        Debug.Log("Creating player @ " + position);
        player = Instantiate(player_prefab, position, Quaternion.identity);
    }

    string entrance_door;
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name + " (GM: " + gameObject.name + ")");
        transition.SetTrigger("End");

        Vector2 player_pos = new Vector2();
        if(respawning)
        {
            player_pos = GameObject.FindGameObjectsWithTag("cairn")[cairn_id].transform.position;
            player_pos.y += 1;
        }
        else
        {
            LevelExit[] entrances = GameObject.FindObjectsOfType<LevelExit>(); // get all of the exits
            Debug.Log("Found " + entrances.Length + " doors");
            LevelExit entrance = null;
            foreach (LevelExit e in entrances)
            {
                if(e.gameObject.name == entrance_door)
                {
                    entrance = e;
                    break;
                }
            }
            if(entrance == null)
            {
                //LevelExit[] entrances = GameObject.FindObjectsOfType<Cairn>(); // get all of the spawns
                //Debug.Log("Found " + entrances.Length + " cairns");
                //foreach (LevelExit e in entrances)
                //{
                //    if (e.gameObject.name == entrance_door)
                //    {
                //        entrance = e;
                //        break;
                //    }
                //}
            }
            // repeat checks for Cairns too
            if (entrance == null)
            {
                Debug.Log("Invalid target door: " + entrance_door);
                spawnPlayer();
                return;
            }
            player_pos = entrance.transform.position;
            // depending on entrance.ExitDir, play different entry animation
        }
        spawnPlayer(player_pos);
        //https://www.youtube.com/watch?v=WFO1GUKYARQ
    }


    // Level Loading
    public Animator transition;
    public float fadeTime = 1f;
    public void LoadLevel(string name, string door_id = "")
    {
        entrance_door = door_id;
        StartCoroutine(LoadLevelCo(name));
    }
    public void LoadLevel(int ind, string door_id = "")
    {
        entrance_door = door_id;
        StartCoroutine(LoadLevelCo(ind));
    }

    IEnumerator LoadLevelCo(string name)
    {
        // play anim
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(fadeTime);
        // Load scene
        SceneManager.LoadScene(name);
    }
    IEnumerator LoadLevelCo(int ind)
    {
        // play anim
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(fadeTime);
        // Load scene
        SceneManager.LoadScene(ind);
    }
}
