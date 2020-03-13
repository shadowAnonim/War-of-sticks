using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Money : MonoBehaviour
{
    private Text text;
    private FirstPersonAIO hero;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        hero = GameObject.Find("Hero").GetComponent<FirstPersonAIO>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Количество монет: " + hero.money;
    }
}
