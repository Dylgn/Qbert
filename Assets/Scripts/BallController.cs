using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public bool isActive = false;
    // Components
    new AudioSource audio;
    Rigidbody body;

    // Movement
    float speed = 1f;
    bool canMove = false;
    int destination = 0;
    int[] parabolaTranslation = new int[2];
    Direction direction = Direction.None;

    // Controllers
    [SerializeField] CubeController cubeController;

    enum Direction
    {
        None,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    void Start()
    {
        audio = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Won't move if ball is falling or it can't move
        if (body.useGravity || !canMove)
            return;
        else
            Move();
    }

    void Move()
    {
        switch (direction)
        {
            case Direction.UpLeft:
                // Increases z (or x) by slight amount each time
                float z = transform.position.z + Time.fixedDeltaTime * speed;
                transform.position = new Vector3(transform.position.x, MoveOnParabola(z), z);
                // Checks if you've reached the destination
                if (z > destination)
                {
                    // Re-aligns coily
                    transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y) + 0.25f, Mathf.Round(transform.position.z));
                    direction = Direction.None;
                }
                break;
            case Direction.UpRight:
                float x = transform.position.x + Time.fixedDeltaTime * speed;
                transform.position = new Vector3(x, MoveOnParabola(x), transform.position.z);
                if (x > destination)
                {
                    transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y) + 0.25f, transform.position.z);
                    direction = Direction.None;
                }
                break;
            case Direction.DownLeft:
                x = transform.position.x - Time.fixedDeltaTime * speed;
                transform.position = new Vector3(x, MoveOnParabola(x), transform.position.z);
                if (x < destination)
                {
                    transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y) + 0.25f, transform.position.z);
                    direction = Direction.None;
                }
                break;
            case Direction.DownRight:
                z = transform.position.z - Time.fixedDeltaTime * speed;
                transform.position = new Vector3(transform.position.x, MoveOnParabola(z), z);
                if (z < destination)
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y) + 0.25f, Mathf.Round(transform.position.z));
                    direction = Direction.None;
                }
                break;
            default: // When no direction is selected (chooses one)
                // Re-alligns object onto play area
                transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y) + 0.25f, transform.position.z);
                // Randomly moves down the map
                if (Random.Range(0, 2) == 0)
                    ChooseDirection(Direction.DownLeft);
                else
                    ChooseDirection(Direction.DownRight);
                break;
        }
        CheckIfOutOfBounds();
    }

    void ChooseDirection(Direction newDirection)
    {
        switch (newDirection)
        {
            case Direction.UpLeft:
                // Translations to use on parabola (used for jumping to another cube)
                parabolaTranslation[0] = (int)transform.position.z;
                parabolaTranslation[1] = (int)(transform.position.y - 0.25f);
                // Gets the end point of the jump
                destination = (int)transform.position.z + 1;
                // Direction you will jump
                direction = Direction.UpLeft;
                // Play jump sound effect
                audio.Play();
                break;
            case Direction.UpRight:
                parabolaTranslation[0] = (int)transform.position.x;
                parabolaTranslation[1] = (int)(transform.position.y - 0.25f);

                destination = (int)transform.position.x + 1;
                direction = Direction.UpRight;
                audio.Play();
                break;
            case Direction.DownLeft:
                parabolaTranslation[0] = (int)transform.position.x - 1;
                parabolaTranslation[1] = (int)(transform.position.y - 1.25f);

                destination = (int)transform.position.x - 1;
                direction = Direction.DownLeft;
                audio.Play();
                break;
            case Direction.DownRight:
                parabolaTranslation[0] = (int)transform.position.z - 1;
                parabolaTranslation[1] = (int)(transform.position.y - 1.25f);

                destination = (int)transform.position.z - 1;
                direction = Direction.DownRight;
                audio.Play();
                break;
        }
    }

    void CheckIfOutOfBounds()
    {
        // Checks if the object is out of bounds
        if (transform.position.x > 4 || transform.position.z > 4)
        {
            destination = 8;
            if (transform.position.x == 8 || transform.position.z == 8)
                ResetMe();
        }
        else if (transform.position.x + transform.position.z < 2)
        {
            destination = -4;
            if (transform.position.x == -4 || transform.position.z == -4)
                ResetMe();
        }
    }

    float MoveOnParabola(float independent)
    {
        // Moves object to another cube using a parabola equation
        float a = 2 + Mathf.Sqrt(3);
        float h = (3 - Mathf.Sqrt(3)) / 2;
        // In Vertex form y = -a(x-h)^2+k   parabolaTranslation is based on player's position
        return (float)(-a * Mathf.Pow(independent - (h + parabolaTranslation[0]), 2) + (1.75 + parabolaTranslation[1]));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("Top") && body.useGravity)
        {
            body.useGravity = false;
            body.isKinematic = true;
            transform.position = new Vector3(Mathf.Round(transform.position.x), 6.25f, Mathf.Round(transform.position.z));

            // Reverts colour if green
            if (transform.gameObject.name == "Green")
            {
                // Colour of cube the ball is on
                Material colour = cubeController.GetColour(collision.transform);
                // Reverts colour 1 tier down
                if (colour == cubeController.GetColour("TOP_SECONDARY"))
                    cubeController.SetColour(collision.transform, cubeController.GetColour("TOP_PRIMARY"));
                else if (colour == cubeController.GetColour("TOP_TERTIARY"))
                    cubeController.SetColour(collision.transform, cubeController.GetColour("TOP_SECONDARY"));
            }
        }
    }

    public void EnableMe(bool a)
    {
        canMove = a;
        body.useGravity = a;
        body.isKinematic = !a;
        isActive = a;
        
    }

    public void ResetMe()
    {
        // Resets movement
        EnableMe(false);
        direction = Direction.None;
        destination = 0;

        // Spawns in random position when reset (2nd highest row)
        if (Random.Range(0, 2) == 1)
            transform.position = new Vector3(4f, 10.2f, 3f);
        else
            transform.position = new Vector3(3f, 10.2f, 4f);
    }
}
