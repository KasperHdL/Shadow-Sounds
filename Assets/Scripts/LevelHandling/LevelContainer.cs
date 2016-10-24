using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class LevelContainer : MonoBehaviour {

    [HideInInspector]
    public LevelContainerData data;

    public GameObject[] obstacles;
//    public GameObject[] posObject;
//    public GameObject[] negObject;
    public AudioSource[] audioSources;
    public LevelStart levelStart;
    public LevelExit levelExit;

    void Start(){
        if(!EditorApplication.isPlaying){
           // EditorApplication.hierarchyWindowChanged += SyncLevelContainer;
            SyncLevelContainer();
        }else if(data != null){
            obstacles = data.obstacles;
            audioSources = data.audioSources;
            levelStart = data.levelStart;
            levelExit = data.levelExit;
        }
    }
    
    void OnEnable(){
        if(!EditorApplication.isPlaying && data == null)
            SyncLevelContainer();
    }

	void SyncLevelContainer () {
        if(this == null)
            return;

        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
//        posObject = GameObject.FindGameObjectsWithTag("PositiveObject");
//        negObject = GameObject.FindGameObjectsWithTag("NegativeObject");
        audioSources = gameObject.GetComponentsInChildren<AudioSource>();

        levelStart = GameObject.FindObjectOfType<LevelStart>();
        levelExit = GameObject.FindObjectOfType<LevelExit>();

        if(data == null)
            data = ScriptableObject.CreateInstance<LevelContainerData>();

        data.obstacles = obstacles;
//        data.posObject = posObject;
//        data.negObject = negObject;
        data.audioSources = audioSources;
        data.levelStart = levelStart;
        data.levelExit = levelExit;
	}
	
}

