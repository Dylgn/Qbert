using UnityEngine;

[CreateAssetMenu(fileName = "New Keybinds", menuName = "Keybinds", order = 52)]
public class Keybinds : ScriptableObject
{
    // Player Controls
    public KeyCode UpLeft = KeyCode.Q;
    public KeyCode UpRight = KeyCode.E;
    public KeyCode DownLeft = KeyCode.A;
    public KeyCode DownRight = KeyCode.D;
}
