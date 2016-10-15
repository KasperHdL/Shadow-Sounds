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
        LoadNextLevel();
    }

    public void LoadNextLevel(){

        if(buildLevelIndex + 1 >= SceneManager.sceneCountInBuildSettings){
            Debug.Log("no more levels");

            return;
        }

        StartCoroutine(LoadScene(++buildLevelIndex));

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

        //move newly loaded level
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

        if(numLevelsLoaded > 0){
            foundContainer.transform.position += levelContainers[numLevelsLoaded].levelExit.transform.position - foundContainer.levelStart.transform.position;
            foundContainer.transform.position += Vector3.up * levelOffset;
        }

        if(numLevelsLoaded == 0)
            Time.timeScale = 1f;
        numLevelsLoaded++;

        if(numLevelsLoaded < currentLevelContainerIndex + numLevelsLoadedAhead)
            LoadNextLevel();

    }
}
