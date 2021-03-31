using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TitleSkip : MonoBehaviour
{

    private VideoPlayer _videoPlayer;
    private PlayersActions _playersActions;
    private AsyncOperation _loadingScene;

    
    void Start()
    {
        _videoPlayer = GetComponentInChildren<VideoPlayer>();
        StartCoroutine(WaitForClipToEnd());
        _loadingScene = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        _loadingScene.allowSceneActivation = false;
        _playersActions = new PlayersActions();
        _playersActions.Enable();

    }

    void Update()
    {
        if (_playersActions.Player1.PauseGame.triggered)
        {
            _loadingScene.allowSceneActivation = true;
        }
    }

    IEnumerator WaitForClipToEnd()
    {
        var waitForSeconds = new WaitForSeconds(0.1f);
        while (!_videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
        }
        while (_videoPlayer.isPlaying)
        {
            yield return waitForSeconds;
        }
        _loadingScene.allowSceneActivation = true;
    }
    
}
