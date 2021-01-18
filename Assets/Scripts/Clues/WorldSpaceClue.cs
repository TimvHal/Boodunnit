using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceClue : MonoBehaviour
{
    public Clue ClueScriptableObject;
    public GameObject Popup;
    public Canvas Canvas;

    public Text ClueText;
    public Text Description;
    public Image ClueImage;

    private List<Clue> _listOfClues;

    private void Awake()
    {      
        _listOfClues = Resources.LoadAll<Clue>("ScriptableObjects/Clues").ToList();
        
        if (SaveHandler.Instance.DoesPlayerHaveClue(ClueScriptableObject.Name))
        {
            //If this throws an error: Check if the clue has an assigned scriptable object
            gameObject.SetActive(false);
        }

        if (!gameObject.GetComponent<Outline>())
        {
            Outline outline = gameObject.AddComponent<Outline>();
            if (outline)
            {
                Color clueColor;
                ColorUtility.TryParseHtmlString("#fabd61", out clueColor);
                outline.OutlineColor = clueColor;
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineWidth = 5.0f;
                outline.enabled = false;
            }
        }

        Canvas = Popup.GetComponent<Canvas>();

        ClueText = Popup.gameObject.transform.Find("Body").Find("TitleBar").Find("ClueTitle_TXT").GetComponent<Text>();
        Description = Popup.gameObject.transform.Find("Body").Find("Description").Find("TextBox").Find("Body_TXT").GetComponent<Text>();
        ClueImage = Popup.gameObject.transform.transform.Find("Body").Find("Description").Find("ClueSprite_Box").Find("Clue_Img").GetComponent<Image>();
    }
    
    public void AddToInventory()
    { 
        //Add this clue to the inventory of the player
        SaveHandler.Instance.SaveClue(ClueScriptableObject.Name);
        SetClueInPopup();

        if (Popup && !DoesPlayerHaveAllCLues()) //todo: remove !DoesPlayerHaveAllCLues() when to be continued popup is not neccisary enymore
        {
            Canvas.enabled = true;
            GameManager.CursorIsLocked = false;
            Time.timeScale = 0f;
        }
    
        SoundManager.Instance.PlaySound("Clue_pickup");
        gameObject.SetActive(false);

        if (DoesPlayerHaveAllCLues())
        {
            SaveHandler.Instance.SaveGameProperty("PlayerHasAllClues", "bool", true);
            GameManager.PlayerHasAllClues = true;
        }
    }

    private bool DoesPlayerHaveAllCLues()
    {
        return _listOfClues.All(clue => SaveHandler.Instance.DoesPlayerHaveClue(clue.Name));
    }

    public void SetClueInPopup()
    {
        ClueText.text = ClueScriptableObject.Name;
        Description.text = ClueScriptableObject.Description;
        ClueImage.sprite = ClueScriptableObject.Image;
    }
}