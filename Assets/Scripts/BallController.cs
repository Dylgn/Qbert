using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public bool isActive = false;
    // Components
    new AudioSource audio;
    Rigidbody body;
    Collider bodyCollider;

    // Movement
    float speed = 1f;
    public bool canMove = false;
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
        bodyCollider = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        // Won't move if ball is falling or it can't move
        if (body.useGravity || !canMove || !isActive)
            return;
        else
            Move();
    }

    void Move()
    {
        // Chooses new direction (if none is selected)
        if (direction == Direction.None)
        {
            // Randomly moves down the map
            if (Random.Range(0, 2) == 0)
                ChooseDirection(Direction.DownLeft);
            else
                ChooseDirection(Direction.DownRight);
        }

        // Indepedent variable used in moving object along parabola
        float independent;
        if (direction == Direction.DownLeft)
            independent = transform.position.x;
        else
            independent = transform.position.z;

        // Moves object
        float shiftedIndependent = independent - Time.fixedDeltaTime * speed;
        if (direction == Direction.DownLeft)
            transform.position = new Vector3(shiftedIndependent, MoveOnParabola(shiftedIndependent), transform.position.z);
        else
            transform.position = new Vector3(transform.position.x, MoveOnParabola(shiftedIndependent), shiftedIndependent);

        // Produces true when you've reached your destination
        if (shiftedIndependent < destination)
        {
            // Re-alligns with centre of cube
            if (direction == Direction.DownLeft)
                transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y) + 0.25f, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y) + 0.25f, Mathf.Round(transform.position.z));
            
            // Reset direction
            direction = Direction.None;
        }
        CheckIfOutOfBounds();
    }

    void ChooseDirection(Direction newDirection)
    {
        // Direction you will jump
        direction = newDirection;

        // Indepedent variable used in determining destination
        float independent;
        if (direction == Direction.DownLeft)
            independent = transform.position.x;
        else
            independent = transform.position.z;

        // Object's destination
        destination = (int)independent - 1;

        // Translations to use on parabola (used for jumping to another cube)
        parabolaTranslation[0] = (int)independent - 1;
        parabolaTranslation[1] = (int)(transform.position.y - 1.25f);

        // Play jump sound effect
        audio.Play();
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
        // In Vertex form y = -a(x-h)^2+k   parabolaTranslation is based on object's position
        return (float)(-a * Mathf.Pow(independent - (h + parabolaTranslation[0]), 2) + (1.75 + parabolaTranslation[1]));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("Top"))
        {
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

            // Disables gravity
            if (body.useGravity)
            {
                body.useGravity = false;
                if (transform.gameObject.name == "Green")
                    bodyCollider.isTrigger = true;
                else
                    body.isKinematic = true;
                transform.position = new Vector3(Mathf.Round(transform.position.x), 6.25f, Mathf.Round(transform.position.z));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.gameObject.name.Equals("Top"))
            return;
        // Reverts colour if ball is green
        // Colour of cube the ball is on
        Material colour = cubeController.GetColour(other.transform);
        // Reverts colour 1 tier down
        if (colour == cubeController.GetColour("TOP_SECONDARY"))
            cubeController.SetColour(other.transform, cubeController.GetColour("TOP_PRIMARY"));
        else if (colour == cubeController.GetColour("TOP_TERTIARY"))
            cubeController.SetColour(other.transform, cubeController.GetColour("TOP_SECONDARY"));
        
    }

    public void EnableMe(bool a)
    {
        canMove = a;
        body.useGravity = a;
        if (transform.gameObject.name == "Green")
            bodyCollider.isTrigger = !a;
        else
            body.isKinematic = !a;
        isActive = a;
        
    }

    public void ResetMe()
    {
        body.velocity = new Vector3(0, 0, 0);
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
