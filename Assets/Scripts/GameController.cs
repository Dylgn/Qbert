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
    [SerializeField] BallController[] redBalls;
    [SerializeField] BallController greenBall;
    [SerializeField] UggController[] ugg;

    [Header("Misc")]
    [SerializeField] Transform characterParent;

    // Enemy Spawning
    string[][] spawnTable = new string[16][];

    // Audio
    new AudioSource audio;
    [SerializeField] AudioClip[] clips;

    int level = 1;
    int points = 0;
    int round = 1;

    bool flashTiles = false;
    bool gameActive = false;

    void Start()
    {
        coily.transform.position = RandomPosAboveLevel(coily.transform.position.y);
        audio = GetComponent<AudioSource>();

        // List of enemies that will spawn each round (Coily spawns every round)
        spawnTable[0] = new string[] { "RED", "UGG" /*TEMP*/ };
        spawnTable[1] = new string[] { "RED" };
        spawnTable[2] = new string[] { "GREEN", "UGG", "SLICK" };
        spawnTable[3] = new string[] { "RED", "GREEN", "SLICK"};
        spawnTable[4] = new string[] { "GREEN", "UGG", "SLICK" };
        spawnTable[5] = new string[] { "GREEN", "UGG", "SLICK" };
        spawnTable[6] = new string[] { "RED", "GREEN", "SLICK" };
        spawnTable[7] = new string[] { "RED", "GREEN", "UGG", "SLICK" };
        spawnTable[8] = new string[] { "RED", "GREEN", "SLICK" };
        spawnTable[9] = new string[] { "GREEN", "UGG", "SLICK" };
        spawnTable[10] = new string[] { "RED", "GREEN", "UGG", "SLICK" };
        spawnTable[11] = new string[] { "RED", "GREEN", "UGG", "SLICK" };
        spawnTable[12] = new string[] { "RED", "GREEN", "SLICK" };
        spawnTable[13] = new string[] { "RED", "GREEN", "UGG", "SLICK" };
        spawnTable[14] = new string[] { "RED", "GREEN", "SLICK" };
        spawnTable[15] = new string[] { "RED", "GREEN", "UGG", "SLICK" }; // Uses this every round after
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

        // Starts new round
        StartRound();
    }

    void Update()
    {
        if (!PlayText.gameObject.activeSelf)
            return;
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // Hides "Press 'Enter' To Play" Text
            PlayText.gameObject.SetActive(false);

            // Starts new round
            StartRound();
        }
    }

    void StartRound()
    {
        // Starts game
        gameActive = true;

        // Makes player visible
        player.canMove = true;
        player.gameObject.SetActive(true);

        // Enemy spawns based off round
        int spawnRound = (level * 4 - 4) + round - 1;
        if (level >= 5)
            spawnRound = 15;

        // Checks if each enemy is in array (then prepares to spawn it)
        if (Array.Exists(spawnTable[spawnRound], e => e.Equals("RED")))
            InvokeRepeating("EnableRedBall", 5f, 5f);
        if (Array.Exists(spawnTable[spawnRound], e => e.Equals("GREEN")))
            Invoke("EnableGreenBall", 14f);
        if (Array.Exists(spawnTable[spawnRound], e => e.Equals("UGG")))
            InvokeRepeating("EnableUgg", 6.5f, 5f);

        // Always enables coily
        Invoke("EnableCoily", 2.5f);
    }

    public void EndRound()
    {
        gameActive = false;

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
        gameActive = false;
        player.canMove = false;
        coily.EnableCoily(false);
        redBalls[0].EnableMe(false);
        redBalls[1].EnableMe(false);
        redBalls[2].EnableMe(false);
        greenBall.EnableMe(false);
        ugg[0].EnableMe(false);
        ugg[1].EnableMe(false);
        ugg[2].EnableMe(false);
        ugg[3].EnableMe(false);


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
        if (gameActive)
            coily.EnableCoily(true);
    }

    void EnableRedBall()
    {
        // Doesn't spawn new balls if game isn't active
        if (!gameActive)
            CancelInvoke("EnableRedBall");
        else if (!redBalls[0].isActive) // Tries to enable the first ball that isn't active
            redBalls[0].EnableMe(true);
        else if (!redBalls[1].isActive)
            redBalls[1].EnableMe(true);
        else
            redBalls[2].EnableMe(true);
    }

    void EnableGreenBall()
    {
        // Cancels if game is no longer active
        if (!gameActive)
            return;
        else if (!greenBall.isActive) // Otherwise, enables the green ball
            greenBall.EnableMe(true);

        Invoke("EnableGreenBall", 10f);
    }

    void EnableUgg()
    {
        // Cancels if game is no longer active
        if (!gameActive)
        {
            CancelInvoke("EnableUgg");
            return;
        }
        else if (!ugg[0].isActive) // Otherwise, enables ugg (or Wrongway)
            ugg[0].EnableMe(true);
        else if (!ugg[1].isActive)
            ugg[1].EnableMe(true);
        else if (!ugg[2].isActive)
            ugg[2].EnableMe(true);
        else if (!ugg[3].isActive)
            ugg[3].EnableMe(true);
    }

    void ResetAllCharacters()
    {
        coily.ResetMe();
        player.ResetMe();

        redBalls[0].ResetMe();
        redBalls[1].ResetMe();
        redBalls[2].ResetMe();

        greenBall.ResetMe();

        ugg[0].ResetMe();
        ugg[1].ResetMe();
        ugg[2].ResetMe();
        ugg[3].ResetMe();

        flashTiles = false;
    }

    public void hitGreen()
    {
        IncreasePoints(100);
        greenBall.ResetMe();
        Invoke("EnableGreenBall", 10f);
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
        // Reset all characters
        gameActive = false;
        ResetAllCharacters();
        // Cancels all invokoe calls
        CancelInvoke();
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
    }
}
