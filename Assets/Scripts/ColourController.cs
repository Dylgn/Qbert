using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Colour Controller", menuName = "Colour Controller", order = 51)]
public class ColourController : ScriptableObject
{
    [SerializeField] Material[] materials;

    public Material[] Top = new Material[3];
    public Material Side;

    public void GetNewColours()
    {
        EmptyMaterials();
        // Gets new materials
        Top[0] = GetRandomMaterial(false);
        Top[1] = GetRandomMaterial(true);
        Side = GetRandomMaterial(true);
        Top[2] = GetRandomMaterial(true);
    }
    void EmptyMaterials()
    {
        Top[0] = null;
        Top[1] = null;
        Top[2] = null;

        Side = null;
        
    }
    Material GetRandomMaterial(bool checkIfInUse)
    {
        Material material = materials[Random.Range(0, materials.Length)];
        // Keeps getting random material until it finds one not already being used
        while (isInUse(material) && checkIfInUse)
        {
            material = materials[Random.Range(0, materials.Length)];
        }
        
        return material;
    }
    bool isInUse(Material material)
    {
        // Checks if material is already in use
        return Top[0] == material || Top[1] == material || Side == material;
    }
}
