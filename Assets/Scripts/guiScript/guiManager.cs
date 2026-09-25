using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class guiManager : MonoBehaviour
{
    // In-game UI
    [Header("Panels (Main Screens)")]
    [Tooltip("Main panels like pausePanel, SettingsGame, CreditGame. " +
             "Only one active at a time.")]
    public List<GameObject> panels = new List<GameObject>();

    [Header("Confirmation Popups (Overlay)")]
    [Tooltip("Confirmation popups like ExitGame, RestartGame, MainMenuConfirm. " +
             "Displayed on top of the active panel. " +
             "Can be a child of a panel — will not disable its parent.")]
    public List<GameObject> confirmationPopups = new List<GameObject>();

    [Header("Buttons")]
    [Tooltip("Add all buttons to this list. " +
             "The GameObject name determines which function is called. " +
             "Example: a button named \"StartNewGame\" will show the popup named \"StartNewGame\".")]
    public List<Button> buttons = new List<Button>();
    private Dictionary<string, Action> buttonActionMap;

    // Pending action for confirmation popup
    // (executed when the user clicks "Yes" on a confirmation popup)
    private Action pendingAction;

    // Start is called before the first frame update
    void Start()
    {
        InitButtonActionMap();
        RegisterButtonListeners();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    private void InitButtonActionMap()
    {
        buttonActionMap = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            // --- Main Menu Buttons (show confirmation popup) ---
            // { "StartNewGame",   () => ShowConfirmPopup("StartNewGame", () => PlayGame(string.Empty)) },
            { "ContinueGame",   () => ShowConfirmPopup("ContinueGame", null) },
            { "SettingsGame",   PopUp_Settings },
            { "CreditGame",     PopUp_Credit },
            { "ExitGame",        () => ShowConfirmPopup("ExitGame", ExitGame) },

            // --- In-Game Buttons ---
            { "ResumeButton",   ResumeGame },
            { "RestartButton",  () => ShowConfirmPopup("RestartGame", RestartGame) },
            { "MainMenuButton", () => ShowConfirmPopup("MainMenuConfirm", GoToMainMenu) },
            // { "PlayButton",     () => PlayGame(string.Empty) },
            { "QuitButton",     () => ShowConfirmPopup("ExitGame", ExitGame) },

            // --- Confirmation Popup Buttons ---
            { "ConfirmYes",     ConfirmYes },
            { "ConfirmNo",      ConfirmNo },
        };
    }

    private void RegisterButtonListeners()
    {
        foreach (Button btn in buttons)
        {
            if (btn == null) continue;
            string buttonName = btn.gameObject.name;
            btn.onClick.AddListener(() => OnButtonClicked(buttonName));
        }
    }

    private void OnButtonClicked(string buttonName)
    {
        if (buttonActionMap.TryGetValue(buttonName, out Action action) && action != null)
        {
            action.Invoke();
            return;
        }
        // Fallback: Shows Pop-Up
        ShowConfirmationPopup(buttonName);
    }


    public void TogglePause()
    {
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 0f;
            ShowPanel("pausePanel");
        }
        else
        {
            Time.timeScale = 1f;
            HideAllPanels();
            HideAllConfirmationPopups();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        ShowPanel("pausePanel");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        HideAllPanels();
        HideAllConfirmationPopups();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        HideAllPanels();
        HideAllConfirmationPopups();

    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }



    public void ShowPanel(string panelName)
    {
        foreach (GameObject panel in panels)
        {
            if (panel == null) continue;
            panel.SetActive(panel.name == panelName);
        }
    }

    public void HideAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }




    public void ShowConfirmationPopup(string popupName)
    {
        foreach (GameObject popup in confirmationPopups)
        {
            if (popup == null) continue;
            popup.SetActive(popup.name == popupName);
        }
    }

    public void HideAllConfirmationPopups()
    {
        foreach (GameObject popup in confirmationPopups)
        {
            if (popup != null) popup.SetActive(false);
        }
    }




    public void ShowConfirmPopup(string popupName, Action onConfirm)
    {
        pendingAction = onConfirm;
        ShowConfirmationPopup(popupName);
    }

    public void ConfirmYes()
    {
        Action toExecute = pendingAction;
        pendingAction = null;
        HideAllConfirmationPopups();
        toExecute?.Invoke();
    }

    public void ConfirmNo()
    {
        pendingAction = null;
        HideAllConfirmationPopups();
    }


    public void PopUp_StartGame()
    {
        ShowConfirmationPopup("StartNewGame");
    }

    public void PopUp_ContinueGame()
    {
        ShowConfirmationPopup("ContinueGame");
    }

    public void PopUp_Settings()
    {
        ShowPanel("SettingsGame");
    }

    public void PopUp_Credit()
    {
        ShowPanel("CreditGame");
    }

    public void PopUp_Quit()
    {
        ShowConfirmationPopup("ExitGame");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}