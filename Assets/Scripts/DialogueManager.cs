using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{

    public ScrollRect scrollRect;
    public ButtonComponent button;
    public string folder = "Dialogues"; // подпапка в Resources, для чтения
    public int offset = 20;

    private string fileName, lastName;
    private List<Dialogue> node;
    private Dialogue dialogue;
    private PlayerAnswer answer;
    private List<RectTransform> buttons = new List<RectTransform>();
    private float curY, height;
    private static DialogueManager _internal;
    private FirstPersonAIO hero;

    public void DialogueStart(string name)
    {
        if (name == string.Empty) return;
        fileName = name;
        Load();
        hero.dialogMode = true;
        hero.enableCameraMovement = false;
        hero.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public static DialogueManager Internal
    {
        get { return _internal; }
    }

    void Awake()
    {
        _internal = this;
        button.gameObject.SetActive(false);
        scrollRect.gameObject.SetActive(false);
        hero = GameObject.Find("Hero").GetComponent<FirstPersonAIO>();
    }

    void Load()
    {
        scrollRect.gameObject.SetActive(true);

        if (lastName == fileName) // проверка, чтобы не загружать уже загруженный файл
        {
            BuildDialogue(0, 0, 0, 0);
            return;
        }

        node = new List<Dialogue>();

        try // чтение элементов XML и загрузка значений атрибутов в массивы
        {
            TextAsset binary = Resources.Load<TextAsset>(folder + "/" + fileName);
            XmlTextReader reader = new XmlTextReader(new StringReader(binary.text));

            int index = 0;
            while (reader.Read())
            {
                if (reader.IsStartElement("node"))
                {
                    dialogue = new Dialogue();
                    dialogue.answer = new List<PlayerAnswer>();
                    dialogue.npcText = reader.GetAttribute("npcText");
                    node.Add(dialogue);

                    XmlReader inner = reader.ReadSubtree();
                    while (inner.ReadToFollowing("answer"))
                    {
                        answer = new PlayerAnswer();
                        answer.text = reader.GetAttribute("text");

                        int number;
                        if (int.TryParse(reader.GetAttribute("toNode"), out number)) answer.toNode = number; else answer.toNode = 0;

                        bool result;
                        if (bool.TryParse(reader.GetAttribute("exit"), out result)) answer.exit = result; else answer.exit = false;

                        int reputation;
                        if (int.TryParse(reader.GetAttribute("reputation"), out reputation)) answer.reputation = reputation; else answer.reputation = 0;

                        int power;
                        if (int.TryParse(reader.GetAttribute("power"), out power)) answer.power = power; else answer.power = 0;

                        int money;
                        if (int.TryParse(reader.GetAttribute("money"), out money)) answer.money = money; else answer.money = 0;

                        bool end;
                        if (bool.TryParse(reader.GetAttribute("end"), out end)) answer.end = end; else answer.end = false;

                        node[index].answer.Add(answer);
                    }
                    inner.Close();

                    index++;
                }
            }

            lastName = fileName;
            reader.Close();
        }
        catch (System.Exception error)
        {
            Debug.Log(this + " Ошибка чтения файла диалога: " + fileName + ".xml >> Error: " + error.Message);
            scrollRect.gameObject.SetActive(false);
            lastName = string.Empty;
        }

        BuildDialogue(0, 0, 0, 0);
    }

    void AddToList(bool exit, int toNode, string text, bool isActive, int reputation, int power, int money, bool end)
    {
        BuildElement(exit, toNode, text, isActive, reputation, power, money, end);
        curY += height + offset;
        RectContent();
    }

    void BuildElement(bool exit, int toNode, string text, bool isActiveButton, int reputation, int power, int money, bool end)
    {
        ButtonComponent clone = Instantiate(button) as ButtonComponent;
        if (hero.money + money < 0) { isActiveButton = false; text += "(Недостаточно денег!)"; }
        clone.gameObject.SetActive(true);
        clone.rect.SetParent(scrollRect.content);
        clone.rect.localScale = Vector3.one;
        clone.text.text = text;
        if (money != 0)
        {
            clone.text.text += " Цена: " + (-money);
        }
        clone.rect.sizeDelta = new Vector2(clone.rect.sizeDelta.x, clone.text.preferredHeight + offset);
        clone.button.interactable = isActiveButton;
        height = clone.rect.sizeDelta.y;
        clone.rect.anchoredPosition = new Vector2(0, -height / 2 - curY);

        if (toNode > 0) SetNextDialogue(clone.button, toNode, reputation, power, money);
        if (exit) SetExitDialogue(clone.button, reputation, power, money, end);
        buttons.Add(clone.rect);
    }

    void RectContent()
    {
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, curY);
        scrollRect.content.anchoredPosition = Vector2.zero;
    }

    void ClearDialogue()
    {
        curY = offset;
        foreach (RectTransform b in buttons)
        {
            Destroy(b.gameObject);
        }
        buttons = new List<RectTransform>();
        RectContent();
    }

    void SetNextDialogue(Button button, int id, int reputation, int power, int money) // добавляем событие кнопке, для перенаправления на другой узел диалога
    {
        button.onClick.AddListener(() => BuildDialogue(id, reputation, power, money));
    }

    void SetExitDialogue(Button button, int reputation, int power, int money, bool end) // добавляем событие кнопке, для выхода из диалога
    {
        button.onClick.AddListener(() => CloseDialogue(reputation, power, money, end));
    }

    void CloseDialogue(int reputation, int power, int money, bool end)
    {
        if (end)
        {
            SceneManager.LoadScene("MainMenu");
        }
        hero.reputation += reputation;
        hero.power += power;
        hero.money += money;
        scrollRect.gameObject.SetActive(false);
        hero.enableCameraMovement = true;
        hero.dialogMode = false;
        ClearDialogue();
    }

    void BuildDialogue(int current, int reputation, int power, int money)
    {
        hero.reputation += reputation;
        hero.power += power;
        hero.money += money;
        ClearDialogue();
        AddToList(false, 0, node[current].npcText, false, 0, 0, 0, false);
        for (int i = 0; i < node[current].answer.Count; i++)
        {
            AddToList(node[current].answer[i].exit, node[current].answer[i].toNode, node[current].answer[i].text, true, node[current].answer[i].reputation, node[current].answer[i].power, node[current].answer[i].money, node[current].answer[i].end);
        }
    }
}

class Dialogue
{
    public string npcText;
    public List<PlayerAnswer> answer;
}
