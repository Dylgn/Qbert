using UnityEngine;

public class CoilyController : MonoBehaviour
{
    public bool isActive = false;
    // Components
    new AudioSource audio;
    Rigidbody body;

    // Movement
    float speed = 1f;
    public bool canMove = false;
    int destination = 0;
    int[] parabolaTranslation = new int[2];
    Direction direction = Direction.None;

    // Objects
    [SerializeField] PlayerController player;
    Transform activeObject; // Active part of coily (either egg or snake)

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
        activeObject = transform.GetChild(0);
        audio = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (body.useGravity || !canMove || !isActive)
            return;
        else
        {
            Move();
            if (direction == Direction.None)
            {
                // Switch forms to snake when egg reaches bottom
                if (activeObject.name.Equals("Egg") && transform.position.y == 1.25)
                {
                    canMove = false;
                    speed = 1.25f;
                    Invoke("SwitchForms", 2);
                }  
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("Top") && body.useGravity && canMove)
        {
            body.useGravity = false;
            body.isKinematic = true;
            transform.position = new Vector3(Mathf.Round(transform.position.x), 6.25f, Mathf.Round(transform.position.z));
        }
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
                if (activeObject.name.Equals("Egg"))
                {
                    // Randomly moves down the map
                    if (Random.Range(0, 2) == 0)
                        ChooseDirection(Direction.DownLeft);
                    else
                        ChooseDirection(Direction.DownRight);
                } else
                {
                    // Difference between coily and players x positions
                    float xDifference = transform.position.x - player.transform.position.x;
                    float zDifference = transform.position.z - player.transform.position.z;

                    // Moves towards player in direction where it's furthest away
                    if (Mathf.Abs(xDifference) == Mathf.Abs(zDifference))
                    {
                        if (transform.position.y > player.transform.position.y)
                            ChooseDirection(Direction.DownRight);
                        else if (transform.position.y < player.transform.position.y)
                            ChooseDirection(Direction.UpLeft);
                        else if (transform.position.x > player.transform.position.x)
                            ChooseDirection(Direction.DownLeft);
                        else // Player is to the left of coily
                            ChooseDirection(Direction.UpRight);
                    }
                    else if (Mathf.Abs(xDifference) > Mathf.Abs(zDifference))
                    {
                        if (xDifference > 0) // Difference is position (coily is further away from centre than player)
                            ChooseDirection(Direction.DownLeft);
                        else
                            ChooseDirection(Direction.UpRight);
                    }
                    else if (Mathf.Abs(xDifference) < Mathf.Abs(zDifference))
                    {
                        if (zDifference > 0)
                            ChooseDirection(Direction.DownRight);
                        else
                            ChooseDirection(Direction.UpLeft);
                    }
                    else
                    {
                        // Chooses UpRight/UpLeft if equidistant (prevents falling off map if on the bottom row)
                        if (xDifference < 0)
                            ChooseDirection(Direction.UpRight);
                        else
                            ChooseDirection(Direction.UpLeft);
                    }
                }
                break;
        }
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

    float MoveOnParabola(float independent)
    {
        // Moves object to another cube using a parabola equation
        float a = 2 + Mathf.Sqrt(3);
        float h = (3 - Mathf.Sqrt(3)) / 2;
        // In Vertex form y = -a(x-h)^2+k   parabolaTranslation is based on object's position
        return (float)(-a * Mathf.Pow(independent - (h + parabolaTranslation[0]), 2) + (1.75 + parabolaTranslation[1]));
    }

    void SwitchForms()
    {
        bool toSnake = activeObject.name.Equals("Egg");
        if (toSnake)
        {
            activeObject = transform.GetChild(1);
            canMove = true;
        } else
            activeObject = transform.GetChild(0);
        // Swaps visibility of coily parts
        transform.GetChild(0).gameObject.SetActive(!toSnake);
        transform.GetChild(1).gameObject.SetActive(toSnake);
    }

    public void EnableCoily(bool a)
    {
        canMove = a;
        body.useGravity = a;
        body.isKinematic = !a;
        isActive = a;
    }

    public void ResetMe()
    {
        CancelInvoke();
        // Spawns in random position when reset (2nd highest row)
        if (UnityEngine.Random.Range(0, 2) == 1)
            transform.position = new Vector3(4f, 10.2f, 3f);
        else
            transform.position = new Vector3(3f, 10.2f, 4f);

        // Resets Coily
        EnableCoily(false);
        direction = Direction.None;
        destination = 0;
        speed = 1;
        activeObject = transform.GetChild(0);
        // Disables snake and enables egg
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(false);
    }
}
