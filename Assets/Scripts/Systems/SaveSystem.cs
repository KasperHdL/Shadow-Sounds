using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;

    public bool playerPickedUpSonar = false;

    public Vector2? PlayerPosition;

    public List<int> PillarsDestroyed = new List<int>();

    public void Awake()
    {
        if (instance != null) DestroyImmediate(gameObject);
        instance = this;
        DontDestroyOnLoad(this);
    }
}
