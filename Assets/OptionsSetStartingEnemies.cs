using System;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class OptionsSetStartingEnemies : MonoBehaviour
{
    [SerializeField] private TMP_Text count;
    private Scrollbar _scrollbar;
    private Entity _waveManager;
    private WaveManagerData _waveManagerData;
    
    void Start()
    {
        _waveManager = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(WaveManagerData)).GetSingletonEntity();
        _scrollbar = GetComponent<Scrollbar>();
        _waveManagerData = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<WaveManagerData>(_waveManager);
        ChangeValue(2);
    }

    public void OnValueChanged()
    {
        var value = _scrollbar.value;
       if (value == 0)
       {
            ChangeValue(2);
       }
       else if (value < 0.3)
       {
           ChangeValue(Mathf.CeilToInt(value*25));
       }
       else if (value < 0.5)
       {
           ChangeValue(Mathf.CeilToInt(value*60));
       }
       else if (value < 0.8)
       {
           ChangeValue(Mathf.CeilToInt(value*150));
       }
       else if (value <= 1)
       {
           ChangeValue(Mathf.CeilToInt(value*1000));
       }
    }

    private void ChangeValue(int value)
    {
        count.text = value.ToString();
        _waveManagerData.StartingAsteroidAmountToSpawn = value;
        World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(_waveManager,_waveManagerData);
    }
}
