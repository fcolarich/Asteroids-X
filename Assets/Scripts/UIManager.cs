using System;
using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{

    [SerializeField] private RectTransform pauseMessage;
    [SerializeField] private TMP_Text player1PointsText;
    [SerializeField] private TMP_Text player2PointsText;
    [SerializeField] private TMP_Text player1LivesText;
    [SerializeField] private TMP_Text player2LivesText;
    [SerializeField] private RectTransform defeatMessage;
    [SerializeField] private TMP_Text defeatLine1;
    [SerializeField] private TMP_Text defeatLine2;
    [SerializeField] private RectTransform welcomeMessage;
    [SerializeField] private GameObject backGround;

    
    public EventHandler OnVideoFinished;
    
    
    private string _player1Points = "0";
    private string _player2Points = "0";
    

    
    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnPause += UIManagerOnPause;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnResume += UIManagerOnResume;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnStart += UIManagerOnStart;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnRestart += UIManagerOnReStart;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UIUpdateSystem>().OnPointsUpdatePlayer1 += UIManagerOnPointsUpdatePlayer1;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UIUpdateSystem>().OnPointsUpdatePlayer2 += UIManagerOnPointsUpdatePlayer2;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UIUpdateSystem>().OnLivesUpdatePlayer1 += UIManagerOnLivesUpdatePlayer1;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UIUpdateSystem>().OnLivesUpdatePlayer2 += UIManagerOnLivesUpdatePlayer2;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerCollisionSystem>().OnPlayersDestroyed += UIManagerOnPlayersDestroyed;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnPlayer2Join += UIManagerOnPlayer2Joined;
        StartCoroutine(ShowScreen());
    }

    IEnumerator ShowScreen()
    {
        var backgroundImage = backGround.GetComponent<Image>();
        var color = backgroundImage.color;
        var appearing = true;
        while (appearing)
        {
            color.a -= 0.005f;
            backgroundImage.color = color;
            yield return new WaitForFixedUpdate();
            
            if (color.a <= 0.1f)
            {
                appearing = false;
                backgroundImage.gameObject.SetActive(false);
            }
        }
    }

    private void UIManagerOnReStart(object sender, EventArgs e)
    {
        defeatMessage.gameObject.SetActive(false);
        player2LivesText.text = "PRESS INTRO TO JOIN";
        player2PointsText.text = "PLAYER 2";
        _player1Points = "0";
        _player2Points = "0";
        player1LivesText.text = "X 3";
        player1PointsText.text = "POINTS";
    }


    private void UIManagerOnStart(object sender, EventArgs e)
    {
        welcomeMessage.gameObject.SetActive(false);
    }

    private void UIManagerOnPlayer2Joined(object sender, EventArgs e)
    {
        player2LivesText.text = "X 3";
        player2PointsText.text = "WELCOME PLAYER 2";
    }

    private void UIManagerOnPlayersDestroyed(object sender, EventArgs e)
    {
        defeatMessage.gameObject.SetActive(true);
        defeatLine1.text = "Player 1 has died with " + _player1Points + " points";
        defeatLine2.text = "Player 2 has died with " + _player2Points + " points";
    }

    private void UIManagerOnLivesUpdatePlayer1(object sender, EventArgs e)
    {
        player1LivesText.text = "X "+ sender;
    }

    private void UIManagerOnLivesUpdatePlayer2(object sender, EventArgs e)
    {
        player2LivesText.text = "X "+sender;
    }

    private void UIManagerOnPointsUpdatePlayer2(object sender, EventArgs e)
    {
        player2PointsText.text = sender.ToString();
        _player2Points = sender.ToString();
    }
    private void UIManagerOnPointsUpdatePlayer1(object sender, EventArgs e)
    {
        player1PointsText.text = sender.ToString();
        _player1Points = sender.ToString();

    }
    private void UIManagerOnResume(object sender, EventArgs e)
    {
        pauseMessage.gameObject.SetActive(false);
    }
    private void UIManagerOnPause(object sender, EventArgs e)
    {
        pauseMessage.gameObject.SetActive(true);
    }

}
