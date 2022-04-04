using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Player player = null;
    public Canvas ui;
    [SerializeField] GameObject player_prefab;
    [SerializeField] GameObject hp_icon_prefab;


    // scene transition management
    bool respawning = true; // respawn the player on first load
    int respawnScene = 0;
    int cairn_id = 0;

    // Track player info across stages
    public int current_hp;

    // Track player progression
    public int max_hp = 5;


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

        ui = transform.GetChild(0).GetComponent<Canvas>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public void onPlayerDeath()
    {
        respawning = true;
        LoadLevel(respawnScene);
    }
    private void playerRespawn()
    {
        current_hp = max_hp;
        GameObject[] cairns = GameObject.FindGameObjectsWithTag("cairn");
        if (cairns.Length > 0 && cairn_id < cairns.Length)
        {
            Vector2 player_pos = cairns[cairn_id].transform.position;
            player_pos.y += 1;
            placePlayer(player_pos);
        }
        else
            placePlayer();

        player.invulnerable = true;
        foreach (Transform child in gm.ui.transform.GetChild(1).transform)
        {
            Destroy(child.gameObject);
        }
        StartCoroutine(BuildHP());
    }

    void placePlayer(Vector2 position = new Vector2(), LevelExit.dir moving_dir = LevelExit.dir.none)
    {
        Debug.Log("Creating player @ " + position);
        player = Instantiate(player_prefab, position, Quaternion.identity).GetComponent<Player>();
    }

    string entrance_door;
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name + " (GM: " + gameObject.name + ")");
        transition.SetTrigger("End");

        if(respawning)
        {
            playerRespawn();
        }
        else
        {
            FindEntrance();
        }
        //https://www.youtube.com/watch?v=WFO1GUKYARQ
    }

    // Called on entering a level *by a doorway*
    void FindEntrance()
    {
        Vector2 player_pos = new Vector2();
        LevelExit[] entrances = FindObjectsOfType<LevelExit>(); // get all of the exits
        Debug.Log("Found " + entrances.Length + " doors");
        LevelExit entrance = null;
        foreach (LevelExit e in entrances)
        {
            if (e.gameObject.name == entrance_door)
            {
                entrance = e;
                break;
            }
        }
        if (entrance == null)
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
            placePlayer();
            return;
        }
        player_pos = entrance.transform.position;
        placePlayer(player_pos);
        // depending on entrance.ExitDir, play different entry animation
    }

    // Level Loading
    public Animator transition;
    public float fadeTime = 1f;
    public void LoadLevel(string name, string door_id = "")
    {
        entrance_door = door_id;
        current_hp = player.hp;
        StartCoroutine(LoadLevelCo(name));
    }
    public void LoadLevel(int ind, string door_id = "")
    {
        entrance_door = door_id;
        current_hp = player.hp;
        StartCoroutine(LoadLevelCo(ind));
    }

    IEnumerator LoadLevelCo(string name)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(name);
    }
    IEnumerator LoadLevelCo(int ind)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(ind);
    }

    IEnumerator BuildHP()
    {
        yield return new WaitForSeconds(1); // wait for fade-in to finish
        List<GameObject> icon_list = new List<GameObject>();
        Transform hp_group = gm.ui.transform.GetChild(1); // hp_group
        for (int i = 0; i < max_hp; ++i)
        {
            GameObject icon = Instantiate(hp_icon_prefab, hp_group);
            icon.GetComponent<RectTransform>().position = new Vector3(100 * i + 75, Screen.height-75, 0); // TODO: make this proportional to screen size
            icon_list.Add(icon);
            player.hp_icons = icon_list;
            yield return new WaitForSeconds(0.2f);
        }
        player.invulnerable = false;
    }
}
