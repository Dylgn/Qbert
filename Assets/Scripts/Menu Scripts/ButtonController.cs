using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class ButtonController : MonoBehaviour
{
    [SerializeField] Keybinds keybinds;
    [SerializeField] Highscore highscore;

    [SerializeField] Text highscoreText;

    [SerializeField] Text mainSelect;
    [SerializeField] Text optionsSelect;

    [SerializeField] GameObject buttons;
    [SerializeField] GameObject options;
    [SerializeField] Text[] bindTexts;

    bool changingKeys = false;
    int toChange = 0;

    void Start()
    {
        highscoreText.text = "Highscore: " + highscore.score + " points";
        Switch(buttons);

        bindTexts[0].text = keybinds.UpLeft.ToString();
        bindTexts[1].text = keybinds.UpRight.ToString();
        bindTexts[2].text = keybinds.DownLeft.ToString();
        bindTexts[3].text = keybinds.DownRight.ToString();
    }

    void Update()
    {
        if (changingKeys)
        {
            GetKeybind();
            return;
        }

        // Determines what select text to use
        Text select = optionsSelect;
        if (GetDisplay() == buttons)
            select = mainSelect;

        // Position of currently slected
        float y = select.rectTransform.anchoredPosition.y;

        // Preforms action of selected button
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.D))
        {
            if (GetDisplay() == buttons)
            {
                // Menu buttons
                switch (y)
                {
                    case 0f:
                        Play();
                        break;
                    case -110f:
                        Options();
                        break;
                    case -220f:
                        Exit();
                        break;
                }
                return;
            } else
            {
                if (y != -285)
                    ChangeKeybind();
                else
                    Return();
                return;
            }
        }

        float offset = 90f;
        if (GetDisplay() == buttons)
            offset = 110;

        // Moves current select
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && y < 0)
            select.rectTransform.anchoredPosition = new Vector2(0, y + offset);
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && y > -285)
            select.rectTransform.anchoredPosition = new Vector2(0, y - offset);
        else
            return;

        // Resizes brackets to fit button name
        if (GetDisplay() == buttons)
        {
            if (select.rectTransform.anchoredPosition.y == -110)
                select.text = "[         ]";
            else
                select.text = "[      ]";
        } else
        {
            switch (select.rectTransform.anchoredPosition.y)
            {
                case 75:
                    select.text = "[             ]";
                    break;
                case -15:
                    select.text = "[              ]";
                    break;
                case -105:
                    select.text = "[                ]";
                    break;
                case -195:
                    select.text = "[                 ]";
                    break;
                case -285:
                    select.text = "[               ]";
                    break;
            }
        }
        
    }

    public void Play()
    {
        SceneManager.LoadScene("Game Scene");
    }

    public void Options()
    {
        Switch(options);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Return()
    {
        Switch(buttons);
    }

    public void ChangeKeybind()
    {
        changingKeys = true;

        // Position of currently slected
        float y = optionsSelect.rectTransform.anchoredPosition.y;

        // Used to tell which key to change
        switch (y)
        {
            case 75:
                toChange = 0;
                break;
            case -15:
                toChange = 1;
                break;
            case -105:
                toChange = 2;
                break;
            case -195:
                toChange = 3;
                break;
        }

        bindTexts[toChange].text = "_";
    }

    void GetKeybind()
    {
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {

            // When user cancels keybind change
            if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(key) && key.ToString().Length > 1 && !key.ToString().EndsWith("Arrow")))
            {
                // Revert text
                switch (toChange)
                {
                    case 0:
                        bindTexts[0].text = keybinds.UpLeft.ToString();
                        break;
                    case 1:
                        bindTexts[1].text = keybinds.UpLeft.ToString();
                        break;
                    case 2:
                        bindTexts[2].text = keybinds.UpLeft.ToString();
                        break;
                    case 3:
                        bindTexts[3].text = keybinds.UpLeft.ToString();
                        break;
                }
                changingKeys = false;
                return;
            } else if (Input.GetKeyDown(key))
            {
                switch (toChange)
                {
                    case 0:
                        keybinds.UpLeft = key;
                        break;
                    case 1:
                        keybinds.UpRight = key;
                        break;
                    case 2:
                        keybinds.DownLeft = key;
                        break;
                    case 3:
                        keybinds.DownRight = key;
                        break;
                }

                // Display key
                if (!key.ToString().EndsWith("Arrow"))
                    bindTexts[toChange].text = key.ToString();
                else
                    bindTexts[toChange].text = key.ToString()[0] + "A";
                changingKeys = false;
                return;
            }
        }
    }

    GameObject GetDisplay()
    {
        if (buttons.activeSelf)
            return buttons;
        else
            return options;
    }

    // Change what to display on menu
    void Switch(GameObject display)
    {
        buttons.SetActive(false);
        options.SetActive(false);

        display.SetActive(true);
    }
}
