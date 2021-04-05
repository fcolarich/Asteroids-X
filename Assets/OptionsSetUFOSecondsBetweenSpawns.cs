using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class OptionsSetUFOSecondsBetweenSpawns : MonoBehaviour
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
        ChangeValue(20);
    }

    public void OnValueChanged()
    {
        var value = _scrollbar.value;
        if (value == 0)
        {
            ChangeValue(1);
        }
        else if (value <= 1)
        {
            ChangeValue((value*100));
        }
    }

    private void ChangeValue(float value)
    {
        count.text = Mathf.CeilToInt(value).ToString();
        _waveManagerData.TimeBetweenTrySpawnsSeconds = value;
        World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(_waveManager,_waveManagerData);
    }
}

