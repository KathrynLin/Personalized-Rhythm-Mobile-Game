using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ItemGenerator : MonoBehaviour
{
    static ItemGenerator instance;
    public static ItemGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    List<GameObject> items = new List<GameObject>();
    public GameObject item;

    int posX = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Init()
    {
        Image cover = item.transform.GetChild(0).GetComponent<Image>();
        TextMeshProUGUI level = item.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI title = item.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI artist = item.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

        foreach (var sheet in GameManager.Instance.sheets)
        {
            cover.sprite = sheet.Value.img;
            level.text = "";
            title.text = sheet.Value.title;
            artist.text = sheet.Value.artist;

            GameObject go = Instantiate(item, transform);
            go.name = sheet.Value.title;
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition3D = new Vector3(posX, 0f, 0f);
            rect.sizeDelta = new Vector2(1920, 1080);
            items.Add(go);

            CreateButton(go, "Button_enter", cover.rectTransform.anchoredPosition, "", OnCoverClick, new Vector2(254, 254), new Color(0,0,0,0));
            CreateButton(go, "Button1", new Vector3(-400, -350, 0), "Prev", OnButton1Click, new Vector2(200,50),new Color(0,0,0,1));
            CreateButton(go, "Button2", new Vector3(400, -350, 0), "Next", OnButton2Click, new Vector2(200,50),new Color(0,0,0,1));

            posX += 1920;
        }
        
    }

    void CreateButton(GameObject parent, string buttonName, Vector3 position, string buttonText, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject buttonObject = new GameObject(buttonName);
        buttonObject.transform.SetParent(parent.transform);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);
        rectTransform.anchoredPosition3D = position;
        rectTransform.localScale = Vector3.one;

        Image image = buttonObject.AddComponent<Image>();
        image.color = Color.black;

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClickAction);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform);

        RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
        textRectTransform.sizeDelta = rectTransform.sizeDelta;
        textRectTransform.anchoredPosition3D = Vector3.zero;
        textRectTransform.localScale = Vector3.one;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
    }

    void OnButton1Click()
    {
        ItemController.Instance.Move(-1);
    }

    void OnButton2Click()
    {
        ItemController.Instance.Move(1);
    }

    void OnCoverClick()
    {
        if (GameManager.Instance.state == GameManager.GameState.Game)
            {
                if (!GameManager.Instance.isPlaying)
                    GameManager.Instance.Play();
            }
            else
            {
                if (!GameManager.Instance.isPlaying)
                    GameManager.Instance.Edit();
            }
    }

    void CreateButton(GameObject parent, string buttonName, Vector3 position, string buttonText, UnityEngine.Events.UnityAction onClickAction, Vector2 size, Color color)
    {
        GameObject buttonObject = new GameObject(buttonName);
        buttonObject.transform.SetParent(parent.transform);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition3D = position;
        rectTransform.localScale = Vector3.one;

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClickAction);

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform);

        RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
        textRectTransform.sizeDelta = rectTransform.sizeDelta;
        textRectTransform.anchoredPosition3D = Vector3.zero;
        textRectTransform.localScale = Vector3.one;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
    }


}