using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Highscore", menuName = "Highscore", order = 53)]
public class Highscore : ScriptableObject
{
    public int score = 0;
}
