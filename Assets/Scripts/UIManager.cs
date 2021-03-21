using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private RectTransform PauseMessage;
    [SerializeField] private TMP_Text Player1Points;
    [SerializeField] private TMP_Text Player2Points;

    
    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnPause += UIManagerOnPause;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnResume += UIManagerOnResume;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerPointsAndLivesManagerSystem>().OnPointsUpdatePlayer1 += UIManagerOnPointsUpdatePlayer1;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerPointsAndLivesManagerSystem>().OnPointsUpdatePlayer2 += UIManagerOnPointsUpdatePlayer2;
    }

    private void UIManagerOnPointsUpdatePlayer2(object sender, EventArgs e)
    {
        Player2Points.text = sender.ToString();
    }

    private void UIManagerOnPointsUpdatePlayer1(object sender, EventArgs e)
    {
        Player1Points.text = sender.ToString();
    }

    private void UIManagerOnResume(object sender, EventArgs e)
    {
        PauseMessage.gameObject.SetActive(false);
    }

    private void UIManagerOnPause(object sender, EventArgs e)
    {
        PauseMessage.gameObject.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
