using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelHandler : MonoBehaviour {


    public AudioHandler audioHandler;
    public Transform player;

    AsyncOperation async;

    public List<LevelContainer> levelContainers;

    public bool removeTestSuitesOnLoad = true;
    public float levelOffset = 10;

    public int numLevelsLoadedAhead = 2;

    int currentLevelContainerIndex = 0;

    int buildLevelIndex = 0;
    int numLevelsLoaded = 0;

	void Start () {
        levelContainers = new List<LevelContainer>();

        LoadNextLevel();
	}

    public void LevelCompleted(){
        currentLevelContainerIndex++;
        player.GetComponent<Player>().resetSize();
        LoadNextLevel();
    }

    public void LoadNextLevel(){

        if(buildLevelIndex + 1 >= SceneManager.sceneCountInBuildSettings){
            Debug.LogWarning("No More Levels");

            return;
        }

        StartCoroutine(LoadScene(++buildLevelIndex));

    }

    public void ResetLevel(){
        var posObject = GameObject.FindGameObjectsWithTag("PositiveObject");
        var negObject = GameObject.FindGameObjectsWithTag("NegativeObject");

        for(int i = 0; i < posObject.Length; i++){
            posObject[i].GetComponent<MeshRenderer>().enabled = true;
            posObject[i].GetComponent<BoxCollider2D>().enabled = true;
        }

        for(int i = 0; i < negObject.Length; i++){
            negObject[i].GetComponent<MeshRenderer>().enabled = true;
            negObject[i].GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    public Vector3 GetResetPosition(){
       
        return levelContainers[currentLevelContainerIndex].levelStart.transform.position;

    }

    IEnumerator LoadScene(int index){
        if(numLevelsLoaded == 0)
            Time.timeScale = 0f;

        async = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);

        while(!async.isDone)
            yield return null;

        //remove test suites
        if(removeTestSuitesOnLoad){
            TestSuite[] testSuites = GameObject.FindObjectsOfType<TestSuite>();

            for(int i = 0; i < testSuites.Length; i++){
                Destroy(testSuites[i].gameObject);

            }
        }

        async.allowSceneActivation = true;

        //find new levelContainer and add it the list of containers
        LevelContainer[] containers = GameObject.FindObjectsOfType<LevelContainer>();
        LevelContainer foundContainer = null;

        if(numLevelsLoaded == 0)
            foundContainer = containers[0];

        for(int i = 0; i < containers.Length; i++){
            for(int j = 0; j < levelContainers.Count; j++){
                if(containers[i] != levelContainers[j]){
                    foundContainer = containers[i];
                    break;
                }
            }

            if(foundContainer != null)
                break;
        }

        levelContainers.Add(foundContainer);
        
        //add audiosources
        audioHandler.sources.AddRange(foundContainer.audioSources);
        foundContainer.levelExit.SetLevelHandler(this);

        //move levelcontainer
        if(numLevelsLoaded > 0){
            foundContainer.transform.position = levelContainers[numLevelsLoaded - 1].levelExit.transform.position;
            foundContainer.transform.position += Vector3.up * levelOffset;
        }

        if(numLevelsLoaded == 0)
            Time.timeScale = 1f;

        numLevelsLoaded++;

        if(numLevelsLoaded < currentLevelContainerIndex + numLevelsLoadedAhead)
            LoadNextLevel();

    }
}
