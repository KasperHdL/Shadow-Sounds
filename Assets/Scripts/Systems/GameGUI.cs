using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameGUI : MonoBehaviour
{
    public PlayerMovement Player;
    public Text HealthText;

    public void Update()
    {
        HealthText.text = "Health: " + Player.health;
    }
}
