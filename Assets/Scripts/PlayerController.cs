using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Keybinds keybinds;
    // Player Controls
    KeyCode UpLeft = KeyCode.Q;
    KeyCode UpRight = KeyCode.E;
    KeyCode DownLeft = KeyCode.A;
    KeyCode DownRight = KeyCode.D;
    // Movement
    Direction direction = Direction.None;
    int destination = 0;
    int[] parabolaTranslation = new int[2];
    public bool canMove = false;
    // Controllers
    [SerializeField] CubeController cubeController;
    [SerializeField] GameController UI;
    // Bool for preventing colour change of first cube
    bool firstCube = true;
    // Audio
    new AudioSource audio;

    void Start()
    {
        audio = GetComponent<AudioSource>();
        Debug.Log("Start");
        UpLeft = keybinds.UpLeft;
        UpRight = keybinds.UpRight;
        DownLeft = keybinds.DownLeft;
        DownRight = keybinds.DownRight;
    }

    enum Direction
    {
        None,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove)
            return;
        else if (direction == Direction.None)
        {
            if (Input.GetKey(UpLeft))
            {
                // Translations to use on parabola (used for jumping to another cube)
                parabolaTranslation[0] = (int)transform.position.z;
                parabolaTranslation[1] = (int)(transform.position.y - 0.25f);
                // Gets the end point of the jump
                destination = (int)transform.position.z + 1;
                // Directiono you will jump
                direction = Direction.UpLeft;
                // Play jump sound effect
                audio.Play();
            }
            else if (Input.GetKey(UpRight))
            {
                parabolaTranslation[0] = (int)transform.position.x;
                parabolaTranslation[1] = (int)(transform.position.y - 0.25f);

                destination = (int)transform.position.x + 1;
                direction = Direction.UpRight;
                audio.Play();
            }
            else if (Input.GetKey(DownLeft))
            {
                parabolaTranslation[0] = (int)transform.position.x - 1;
                parabolaTranslation[1] = (int)(transform.position.y - 1.25f);

                destination = (int)transform.position.x - 1;
                direction = Direction.DownLeft;
                audio.Play();
            }
            else if (Input.GetKey(DownRight))
            {
                parabolaTranslation[0] = (int)transform.position.z - 1;
                parabolaTranslation[1] = (int)(transform.position.y - 1.25f);

                destination = (int)transform.position.z - 1;
                direction = Direction.DownRight;
                audio.Play();
            }
        }
    }
    void FixedUpdate()
    {
        // Moves player along parabola until it reaches the next cube
        switch (direction)
        {
            case Direction.UpLeft:
                // Increases z (or x) by slight amount each time
                float z = transform.position.z + Time.fixedDeltaTime * 2;
                transform.position = new Vector3(transform.position.x, MovePlayer(z), z);
                // Checks if you've reached the destination
                if (z > destination)
                {
                    // Re-aligns the plyaer
                    transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y) + 0.25f, Mathf.Round(transform.position.z));
                    direction = Direction.None;
                }
                break;
            case Direction.UpRight:
                float x = transform.position.x + Time.fixedDeltaTime * 2;
                transform.position = new Vector3(x, MovePlayer(x), transform.position.z);
                if (x > destination)
                {
                    transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y) + 0.25f, transform.position.z);
                    direction = Direction.None;
                }
                break;
            case Direction.DownLeft:
                x = transform.position.x - Time.fixedDeltaTime * 2;
                transform.position = new Vector3(x, MovePlayer(x), transform.position.z);
                if (x < destination)
                {
                    transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y) + 0.25f, transform.position.z);
                    direction = Direction.None;
                }
                break;
            case Direction.DownRight:
                z = transform.position.z - Time.fixedDeltaTime * 2;
                transform.position = new Vector3(transform.position.x, MovePlayer(z), z);
                if (z < destination)
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y) + 0.25f, Mathf.Round(transform.position.z));
                    direction = Direction.None;
                }
                break;
        }
        CheckIfOutOfBounds();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!canMove)
            return;
        else if (firstCube)
        {
            firstCube = false;
            return;
        }
        
        // Freeze enemies when hitting the green ball
        if (collision.gameObject.name.Equals("Green"))
            StartCoroutine(UI.freeze());
        // Resets game against all other enemies
        else if (!collision.gameObject.name.Equals("Top") && !collision.gameObject.name.Equals("Left") && !collision.gameObject.name.Equals("Right"))
            UI.ResetGame();
        // Checks if x and z coordinates are very close (to change colour of cube that the player jumps on)
        else if (Mathf.Abs(collision.transform.position.x - transform.position.x) < 0.01f && Mathf.Abs(collision.transform.position.z - transform.position.z) < 0.01f)
        {
            // Colour of cube player is standing on
            Material colour = cubeController.GetColour(collision.transform);
            // Changes colour based on what the colour is and what level (level 2 has 3 colours)
            if (colour == cubeController.GetColour("TOP_PRIMARY"))
                cubeController.SetColour(collision.transform, cubeController.GetColour("TOP_SECONDARY"));
            else if (colour == cubeController.GetColour("TOP_SECONDARY"))
            {
                // Changes to tertiary if level 2, switches back to primary if level 3 or higher
                if (UI.GetLevel() == 2)
                    cubeController.SetColour(collision.transform, cubeController.GetColour("TOP_TERTIARY"));
                else if (UI.GetLevel() > 2)
                    cubeController.SetColour(collision.transform, cubeController.GetColour("TOP_PRIMARY"));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Freeze enemies when hitting the green ball
        if (other.gameObject.name.Equals("Green"))
            StartCoroutine(UI.freeze());
    }

    void CheckIfOutOfBounds()
    {
        // Checks if the player is out of bounds
        if (transform.position.x > 4 || transform.position.z > 4)
        {
            destination = 8;
            if (transform.position.x == 8 || transform.position.z == 8)
                UI.ResetGame();
        } else if (transform.position.x + transform.position.z < 2)
        {
            destination = -4;
            if (transform.position.x == -4 || transform.position.z == -4)
                UI.ResetGame();
        }
    }

    float MovePlayer(float independent)
    {
        // Moves player to another cube using a parabola equation
        float a = 2 + Mathf.Sqrt(3);
        float h = (3 - Mathf.Sqrt(3)) / 2;
        // In Vertex form y = -a(x-h)^2+k   parabolaTranslation is based on player's position
        return (float)(-a * Mathf.Pow(independent - (h + parabolaTranslation[0]), 2) + (1.75 + parabolaTranslation[1]));
    }

    public void ResetMe()
    {
        // Resets player
        transform.position = new Vector3(4f, 7.25f, 4f);
        canMove = false;
        direction = Direction.None;
        destination = 0;
        firstCube = true;
        transform.gameObject.SetActive(false);
    }
}
