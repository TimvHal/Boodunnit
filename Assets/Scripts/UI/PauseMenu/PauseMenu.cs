using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject Canvas;
    public GameObject SettingsCanvas;
    public CameraController CameraController;

    public List<GameObject> CanvasPanels;

    public List<GameObject> ClueImages = new List<GameObject>();

    public void TogglePauseGame()
    {
        if (GameManager.IsPaused)
        {
            ResetPanels();
        }

        GameManager.IsPaused = !GameManager.IsPaused;
        if(!ConversationManager.HasConversationStarted)
            GameManager.CursorIsLocked = !GameManager.CursorIsLocked;
        Canvas.SetActive(GameManager.IsPaused);
        Time.timeScale = GameManager.IsPaused ? 0 : 1;

        //Hint: dont forget to check if player settings is not null, this just threw an error because it was not checked
        PlayerSettings playerSettings = SaveHandler.Instance.LoadDataContainer<PlayerSettings>();
        if (playerSettings != null)
        {
            CameraController.RotationSpeed = playerSettings.CameraSensitivity;
        }

        ToggleImagesCollectedClues();
    }

    private void ToggleImagesCollectedClues()
    {
        List<Clue> clues = Resources.LoadAll<Clue>("ScriptableObjects/Clues").ToList();
        foreach (Clue clue in clues)
        {
            if (SaveHandler.Instance.DoesPlayerHaveClue(clue.Name))
            {
                foreach (GameObject clueImageButton in ClueImages)
                {
                    if (clueImageButton.name.Equals(clue.Name))
                    {
                        clueImageButton.GetComponent<Image>().color = Color.white;
                        clueImageButton.GetComponent<Button>().interactable = true;
                    }
                }
            }
        }
    }

    public void OnQuitToMainMenu()
    {
        TogglePauseGame();
        SceneTransitionHandler.Instance.GoToMainMenu();
    }

    private void ResetPanels()
    {
        SettingsCanvas.SetActive(false);

        foreach (GameObject panel in CanvasPanels)
        {
            panel.SetActive(false);
        }

        CanvasPanels[0].SetActive(true);
    }
}
