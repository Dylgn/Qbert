using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class GameController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] Text pointsText;
    [SerializeField] Text levelText;
    [SerializeField] Text roundText;
    [SerializeField] Image targetColourImage;
    [SerializeField] Text PlayText;

    [Header("Controllers")]
    [SerializeField] ColourController colour;
    [SerializeField] CubeController cube;
    [SerializeField] PlayerController player;
    [SerializeField] CoilyController coily;

    [Header("Misc")]
    [SerializeField] Transform characterParent;

    // Audio
    new AudioSource audio;
    [SerializeField] AudioClip[] clips;

    int level = 1;
    int points = 0;
    int round = 1;

    bool flashTiles = false;

    void Start()
    {
        coily.transform.position = RandomPosAboveLevel(coily.transform.position.y);
        audio = GetComponent<AudioSource>();
    }

    void NewRound()
    {
        // Changes level & round
        if (round == 4)
        {
            ++level;
            round = 1;
        } else
        {
            ++round;
        }
        levelText.text = level.ToString();
        roundText.text = round.ToString();

        // Updates colours on game
        colour.GetNewColours();
        cube.updateColours(0);

        // Only gets the third colour on level 2 (is unused elsewhere)
        if (level == 2)
            targetColourImage.material = colour.Top[2];
        else
            targetColourImage.material = colour.Top[1];

        // Makes player visible
        player.canMove = true;
        player.gameObject.SetActive(true);

        Invoke("EnableCoily", 2.5f);
    }

    void Update()
    {
        if (!PlayText.gameObject.activeSelf)
            return;
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            player.canMove = true;
            player.gameObject.SetActive(true);
            PlayText.gameObject.SetActive(false);
            Invoke("EnableCoily", 2.5f);
        }
    }

    public void EndRound()
    {
        // Points gained from round
        int bonus = 1000 * level;
        // Caps at 5000 per round
        if (bonus > 5000)
            bonus = 5000;
        else
        {
            // Extra bonus for rounds (except after level 5)
            bonus += (round - 1) * 250;
        }

        IncreasePoints(bonus);
        // Freezes characters
        player.canMove = false;
        coily.canMove = false;

        // Plays different audio for advanced to the next round or level
        if (round == 4)
            PlaySound("next-level");
        else
            PlaySound("next-round");
        
        // Resets all chaaracters after 2s
        Invoke("ResetAllCharacters", 2f);
        Invoke("NewRound", 3.5f);
        // Plays flashing tile animation
        flashTiles = true;
        StartCoroutine(FlashingTileAnimation());
    }

    void EnableCoily()
    {
        coily.EnableCoily();
    }

    void ResetAllCharacters()
    {
        // Hides and resets position of all characters in the game
        foreach (Transform parent in characterParent)
        {
            foreach (Transform child in parent)
            {
                if (!child.gameObject.activeSelf)
                    continue;
                else if (child.name == "Snake" || child.name == "Egg") // Coily's position is stored in the parent and therefore has to be managed differently
                {
                    child.gameObject.SetActive(false);
                    continue;
                }
                // Find starting position of child
                switch (child.name)
                {
                    case "Ugg":
                        child.position = new Vector3(4, 0.5f, -6.675f);
                        break;
                    case "Wrongway":
                        child.position = new Vector3(-6.675f, 0.5f, 4);
                        break;
                    default:
                        child.position = RandomPosAboveLevel(child.position.y);
                        break;
                }
                // Disables character
                child.gameObject.SetActive(false);
            }
            // Coily's position is stored in the parent and therefore has to be managed differently
            if (parent.name == "Coily")
                coily.ResetMe(); //parent.position = RandomPosAboveLevel(parent.position.y);
            else if (parent.name == "Qbert")
            {
                player.ResetMe();
            }
        }
        flashTiles = false;
    }

    Vector3 RandomPosAboveLevel(float y)
    {
        // Returns a vector3 chosen from 2 values above the level (used for respawning objects)
        switch (UnityEngine.Random.Range(0,2))
        {
            case 0:
                return new Vector3(3, y, 4);
            default:
                return new Vector3(4, y, 3);
        }
    }

    IEnumerator FlashingTileAnimation()
    {
        int top = 0;
        while (flashTiles)
        {
            yield return new WaitForSeconds(0.1f);
            switch (top)
            {
                case 0:
                    cube.updateColours(0);
                    ++top;
                    break;
                case 1:
                    cube.updateColours(1);
                    ++top;
                    break;
                case 2:
                    cube.updateColours(2);
                    top = 0;
                    break;
            }
        }
    }

    public void PlaySound(string name)
    {
        // Plays sound with string name
        audio.clip = Array.Find<AudioClip>(clips, clip => clip.name == name);
        audio.Play();
    }

    public void IncreasePoints(int increment)
    {
        points += increment;
        pointsText.text = points.ToString();
    }

    public int GetLevel()
    {
        return level;
    }

    public void ResetGame()
    {
        // Resets UI Elements
        points = 0;
        pointsText.text = "0";
        level = 1;
        levelText.text = "1";
        round = 1;
        roundText.text = "1";
        PlayText.gameObject.SetActive(true);
        // Updates colours on game
        colour.GetNewColours();
        cube.updateColours(0);
        cube.ResetCubeTarget();
        // Reset all characters
        ResetAllCharacters();
    }
}
