using System;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    [SerializeField] ColourController colour;
    Cube[] cubes = new Cube[28];

    int targetCubes = 0;

    [SerializeField] GameController UI;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            cubes[i] = transform.GetChild(i).GetComponent<Cube>();
        }
        // Gets random colour on startup
        colour.GetNewColours();
        updateColours(0);
    }

    public void CheckWin()
    {
        if (targetCubes == 28)
        {
            targetCubes = 0;
            UI.EndRound();
        }
    }

    public void updateColours(int index)
    {
        // Updates the faces of all cubes with the current material colour
        foreach (Cube cube in cubes)
        {
            cube.Top.material = colour.Top[index];
            cube.Left.material = colour.Side;
            cube.Right.material = colour.Side;
        }
    }

    public Material GetColour(Transform cubeTransform)
    {
        // Returns the colour of the cube at the specified location
        foreach (Cube cube in cubes)
        {
            if (cube.transform == cubeTransform.parent)
            {
                return cube.Top.sharedMaterial;
            }
        }
        // If no cube is found with the given transform
        throw new NullReferenceException();
    }

    public Material GetColour(string cubeFace)
    {
        switch (cubeFace.ToUpper())
        {
            case "TOP_PRIMARY":
                return colour.Top[0];
            case "TOP_SECONDARY":
                return colour.Top[1];
            case "TOP_TERTIARY":
                return colour.Top[2];
            default:
                return colour.Side;
        }
    }

    public void SetColour(Transform cubeTransform, Material material)
    {
        // Sets the top material to the given material of the given transform
        foreach (Cube cube in cubes)
        {
            if (cube.transform == cubeTransform.parent)
            {
                // Changes target colour count
                if (material == colour.Top[1] && UI.GetLevel() != 2) // Ascending from 1 to 2
                {
                    ++targetCubes;
                    UI.IncreasePoints(25);
                }
                else if (material == colour.Top[2] && UI.GetLevel() == 2) // Ascending from 2 to 3
                {
                    ++targetCubes;
                    UI.IncreasePoints(25);
                }
                else if (material == colour.Top[0]) // Descending from 2 to 1
                    --targetCubes;
                else if (material == colour.Top[1] && cube.Top.sharedMaterial == colour.Top[2] && UI.GetLevel() == 2) // Descending from 3 to 2
                    --targetCubes;
                else if (material == colour.Top[1] && UI.GetLevel() == 2) // Ascending from 1 to 2
                    UI.IncreasePoints(15);

                // Changes material
                cube.Top.material = material;
                CheckWin();
                return;
            }
        }
        // If no cube is found with the given transform
        throw new NullReferenceException();
    }
}
