﻿using UnityEngine;
using System.Collections;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;

    public Vector2? PlayerPosition;

    public void Awake()
    {
        if (instance != null) DestroyImmediate(gameObject);
        instance = this;
        DontDestroyOnLoad(this);
    }
}
