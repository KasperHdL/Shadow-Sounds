using UnityEngine;
using System.Collections;

public class LevelContainerData : ScriptableObject{

    public GameObject[] obstacles;
    public GameObject[] posObject;
    public GameObject[] negObject;
    public AudioSource[] audioSources;
    public LevelStart levelStart;
    public LevelExit levelExit;

}
