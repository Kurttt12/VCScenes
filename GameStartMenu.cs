using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;
    public GameObject start;         // This panel now hosts the updated play menu buttons.
    public GameObject tutorial;
    public GameObject settings;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button tutorialButton;
    public Button settingsButton;
    public Button quitButton;

    public List<Button> returnButtons;

    // --- Updated Play Menu Buttons ---
    [Header("Play Menu Additional Buttons")]
    public Button preModulesButton;
    public Button lessonModulesButton;
    public Button assessmentModulesButton;
    public Button cutScenesButton;   // New cutscenes button
    public Button fullExperienceButton; // New full experience button

    [Header("Module Objects")]
    public GameObject preModulesObject;
    public GameObject lessonModulesObject;
    public GameObject assessmentModulesObject;
    public GameObject cutScenesObject; // New cutscenes object
    public GameObject fullExperienceObject; // New full experience object

    [Header("Module Back Buttons")]
    public List<Button> moduleBackButtons; // Back buttons for module GameObjects

    // -----------------------------------------------------------------------------------------

    void Start()
    {
        EnableMainMenu();

        // Hook events for main menu buttons (unchanged)
        startButton.onClick.AddListener(EnablePlay);
        tutorialButton.onClick.AddListener(EnableTutorial);
        settingsButton.onClick.AddListener(EnableSettings);
        quitButton.onClick.AddListener(QuitGame); 
        
        foreach (var item in returnButtons)
        {
            item.onClick.AddListener(EnableMainMenu);
        }

        // Hook events for the play menu module buttons
        preModulesButton.onClick.AddListener(() => ShowModule(preModulesObject));
        lessonModulesButton.onClick.AddListener(() => ShowModule(lessonModulesObject));
        assessmentModulesButton.onClick.AddListener(() => ShowModule(assessmentModulesObject));
        cutScenesButton.onClick.AddListener(() => ShowModule(cutScenesObject)); // Hooking new cutscenes button

        // Hook event for the full experience button (now with an associated object)
        fullExperienceButton.onClick.AddListener(EnableFullExperience);

        // Hook events for the module back buttons; each will return the UI to the Start panel.
        if (moduleBackButtons != null)
        {
            foreach (var backButton in moduleBackButtons)
            {
                if (backButton != null)
                    backButton.onClick.AddListener(ReturnToPlayMenu);
            }
        }
    }

    public void HideAll()
    {
        mainMenu.SetActive(false);
        start.SetActive(false);
        tutorial.SetActive(false);
        settings.SetActive(false);
        // Hide all module objects as well
        preModulesObject.SetActive(false);
        lessonModulesObject.SetActive(false);
        assessmentModulesObject.SetActive(false);
        cutScenesObject.SetActive(false);
        fullExperienceObject.SetActive(false);
    }

    public void EnableMainMenu()
    {
        mainMenu.SetActive(true);
        start.SetActive(false);
        tutorial.SetActive(false);
        settings.SetActive(false);
        // Ensure that any open module object is hidden when returning to the main menu
        preModulesObject.SetActive(false);
        lessonModulesObject.SetActive(false);
        assessmentModulesObject.SetActive(false);
        cutScenesObject.SetActive(false);
        fullExperienceObject.SetActive(false);
    }

    public void EnablePlay()
    {
        mainMenu.SetActive(false);
        start.SetActive(true);
        tutorial.SetActive(false);
        settings.SetActive(false);
        // Hide module objects when showing the play menu
        preModulesObject.SetActive(false);
        lessonModulesObject.SetActive(false);
        assessmentModulesObject.SetActive(false);
        cutScenesObject.SetActive(false);
        fullExperienceObject.SetActive(false);
    }

    public void EnableTutorial()
    {
        mainMenu.SetActive(false);
        start.SetActive(false);
        tutorial.SetActive(true);
        settings.SetActive(false);
    }

    public void EnableSettings()
    {
        mainMenu.SetActive(false);
        start.SetActive(false);
        tutorial.SetActive(false);
        settings.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // --- Updated ShowModule Method ---
    public void ShowModule(GameObject moduleObj)
    {
        start.SetActive(false);
        moduleObj.SetActive(true);
    }

    // --- New Full Experience Method ---
    public void EnableFullExperience()
    {
        start.SetActive(false);
        fullExperienceObject.SetActive(true);
    }

    // --- New Method for Module Back Buttons ---
    public void ReturnToPlayMenu()
    {
        // Hide all module objects.
        preModulesObject.SetActive(false);
        lessonModulesObject.SetActive(false);
        assessmentModulesObject.SetActive(false);
        cutScenesObject.SetActive(false);
        fullExperienceObject.SetActive(false);
        // Activate the play menu (start UI)
        start.SetActive(true);
    }
}
