using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : ActionableGeneric
{
    public override void fireInput(int i) 
    {
        switch(i)
        {
            case 0: talk();
                break;
            case 1: exit();
                break;
        }
    }

    [SerializeField]
    GameObject displayField;
    private void Awake()
    {
        // displayField = GameObject.Find("NPC text obj");
        for(int i = 0; i < pages.Length; ++i)
        {
            pages[i] = pages[i].Replace("\\n", "\n");
        }
    }

    [SerializeField] string[] pages;
    int current_page = 0;
    void talk()
    {
        
        if (current_page >= pages.Length)
        {
            exit();
            return;
        }
        //displayField.enabled = true;
        displayField.SetActive(true);
        displayField.GetComponent<TextMesh>().text = pages[current_page];
        current_page++;
    }

    void exit()
    {
        current_page = 0;
        //displayField.enabled = false;
        displayField.SetActive(false);
    }
}
