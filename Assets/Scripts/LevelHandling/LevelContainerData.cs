using UnityEngine;
using System.Collections;

public class LevelContainerData : ScriptableObject{

    public GameObject[] obstacles;
    public AudioSource[] audioSources;
    public LevelStart levelStart;
    public LevelExit levelExit;

}
