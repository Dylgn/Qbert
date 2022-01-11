using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UggController : MonoBehaviour
{
    public bool isActive = false;
    // Components
    new AudioSource audio;
    Rigidbody body;

    // Movement
    float speed = 1f;
    public bool canMove = false;
    float destination = 0;
    float[] parabolaTranslation = new float[2];
    Direction direction = Direction.None;
    public bool falling = false;

    // Used to determine which cube face the enemy jumps on
    [SerializeField] bool onLeft = true;

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
        if (!canMove)
            return;
        else if (falling)
        {
            // Fall in direction based on if ugg is alligned to the left of the cube or right
            if (onLeft)
                body.velocity = new Vector3(body.velocity.x + (9.8f * Time.deltaTime), 0, 0);
            else
                body.velocity = new Vector3(0, 0, body.velocity.z + (9.8f * Time.deltaTime));
            // Different order of position variables based on allignment
        } else if (onLeft)
            Move(transform.position.z, transform.position.x, transform.position.y);
        else
            Move(transform.position.y, transform.position.z, transform.position.x);

    }

    // Rotated position variables to jump on side of cubes correctly
    void Move(float rotX, float rotY, float rotZ)
    {
        // Chooses new direction (if none is selected)
        if (direction == Direction.None)
        {
            if (Random.Range(0,2) == 0)
                ChooseDirection(Direction.DownLeft, rotX, rotY);
            else
                ChooseDirection(Direction.DownRight, rotZ, rotY);
        }

        // Indepedent variable used in moving object along parabola
        float independent = 0f;
        if (direction == Direction.DownLeft)
            independent = rotX;
        else
            independent = rotZ;

        // Direction to move independent variable
        int dir = 1;
        if (direction == Direction.DownRight ^ onLeft)
            dir *= -1;

        // Moves object
        float shiftedIndependent = independent + (Time.fixedDeltaTime * speed * dir);
        if (direction == Direction.DownLeft)
            transform.position = RotateMove(shiftedIndependent, MoveOnParabola(shiftedIndependent), rotZ);
        else
            transform.position = RotateMove(rotX, MoveOnParabola(shiftedIndependent), shiftedIndependent);

        // Produces true when you've reached your destination
        if (onLeft ^ direction == Direction.DownRight ^ shiftedIndependent > destination)
        {
            // Changes to correct re-allignment
            float finalIndepedent = Mathf.Round(independent);
            if (direction == Direction.DownLeft ^ onLeft)
                finalIndepedent += 0.5f;

            // Re-alligns with centre of cube
            if (direction == Direction.DownLeft)
                transform.position = RotateMove(finalIndepedent, Mathf.Round(rotY) + 0.25f, rotZ);
            else
                transform.position = RotateMove(rotX, Mathf.Round(rotY) + 0.25f, finalIndepedent);

            // Resets direction
            direction = Direction.None;
        }

        // Checks if object is out of bounds and resets it if it is
        CheckIfOutOfBounds(rotY);
        /*switch (direction)
        {
            case Direction.DownLeft:
                float x = rotX + Time.fixedDeltaTime * speed;
                //Debug.Log(x + " " + MoveOnParabola(x) + " " + rotZ + " / " + parabolaTranslation[0] + " " + parabolaTranslation[1]);
                transform.position = RotateMove(x, MoveOnParabola(x), rotZ);
                if (x > destination)
                {
                    transform.position = RotateMove(Mathf.Round(rotX) + 0.5f /*PREV: 0, Mathf.Round(rotY) + 0.25f, rotZ);
                    direction = Direction.None;
                }
                break;
            case Direction.DownRight:
                float z = rotZ - Time.fixedDeltaTime * speed;
                //Debug.Log(rotX + " " + MoveOnParabola(z) + " " + z + " / " + parabolaTranslation[0] + " " + parabolaTranslation[1]);
                transform.position = RotateMove(rotX, MoveOnParabola(z), z);
                if (z < destination)
                {
                    transform.position = RotateMove(rotX, Mathf.Round(rotY) + 0.25f, Mathf.Round(rotZ) /*+ 0.5f);
                    direction = Direction.None;
                }
                break;
            default: // When no direction is selected (chooses one)
                // Re-alligns object onto play area
                transform.position = RotateMove(rotX, Mathf.Floor(rotY) + 0.25f, rotZ);
                // Randomly moves down the map
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    if (onLeft)
                        ChooseDirection(Direction.DownLeft, rotX, rotY);
                    else
                        ChooseDirection(Direction.DownLeft, rotX, rotY);
                } else
                {
                    if (onLeft)
                        ChooseDirection(Direction.DownRight, rotZ, rotY);
                    else
                        ChooseDirection(Direction.DownRight, rotZ, rotY);
                }
                /*    ChooseDirection(Direction.DownLeft, rotX, rotY, rotZ);
                else
                    ChooseDirection(Direction.DownRight, rotX, rotY, rotZ);
                Debug.Log(direction);
                break;
        }
        CheckIfOutOfBounds(rotY);*/
    }

    void ChooseDirection(Direction newDirection, float independent, float rotY)
    {
        parabolaTranslation[1] = rotY - 0.5f;
        audio.Play();
        direction = newDirection;

        if (newDirection == Direction.DownLeft ^ onLeft)
        {
            parabolaTranslation[0] = independent + 1f;
            destination = independent + 1f;
        } else
        {
            parabolaTranslation[0] = independent - 1f;
            destination = independent - 1f;
        }
        /*switch (newDirection)
        {
            case Direction.DownLeft:
                // Translations to use on parabola (used for jumping to another cube)
                parabolaTranslation[0] = rotX - 1f;
                parabolaTranslation[1] = rotY - 0.5f;
                // Gets the end point of the jump
                destination = rotX - 1;
                // Direction you will jump
                direction = Direction.DownLeft;
                // Play jump sound effect
                audio.Play();
                break;
            case Direction.DownRight:
                parabolaTranslation[0] = rotZ + 1f;
                parabolaTranslation[1] = rotY - 0.5f;

                destination = rotZ + 1;
                direction = Direction.DownRight;
                audio.Play();
                break;
        }*/
    }

    void CheckIfOutOfBounds(float rotY)
    {
        if (rotY > 3.5)
        {
            if (direction == Direction.DownLeft ^ onLeft)
                destination = 10;
            else
                destination = -10;

            if (rotY > 6.5)
                ResetMe();
        }
    }

    float MoveOnParabola(float independent)
    {
        // Moves object to another cube using a parabola equation
        float a = 2 + Mathf.Sqrt(3);
        float h = (3 - Mathf.Sqrt(3)) / 2;
        if (direction == Direction.DownLeft ^ onLeft)
            h *= -1;
        // In Vertex form y = a(x-h)^2+k   parabolaTranslation is based on object's position
        return (float)(a * Mathf.Pow(independent - (h + parabolaTranslation[0]), 2) + parabolaTranslation[1]);
    }

    // Moves transform correctly based on 'onLeft' variable
    Vector3 RotateMove(float rotX, float rotY, float rotZ)
    {
        if (onLeft)
            return new Vector3(rotY, rotZ, rotX);
        else
            return new Vector3(rotZ, rotX, rotY);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Stops Ugg from falling after it reaches game area
        if ((collision.gameObject.name.Equals("Left") || collision.gameObject.name.Equals("Right")) && falling)
        {
            falling = false;
            if (onLeft)
                transform.position = new Vector3(-2.75f, 0.5f, 4f);
            else
                transform.position = new Vector3(4f, 0.5f, -2.75f);
        }
    }

    public void EnableMe(bool a)
    {
        falling = a;
        canMove = a;
        isActive = a;
    }

    public void ResetMe()
    {
        // Resets movement
        EnableMe(false);
        direction = Direction.None;
        destination = 0;

        // Spawns in random position when reset (2nd highest row)
        if (onLeft)
            transform.position = new Vector3(-6.675f, 0.5f, 4f);
        else
            transform.position = new Vector3(4f, 0.5f, -6.675f);
    }
}
