using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageMeterManager : MainGameplayManagerFramework
{
    public static DamageMeterManager Instance;

    [Header("Assignments")]
    [SerializeField]
    private GameObject _metersParent;
    [SerializeField]
    private TMP_Text _timerText;
    [Space]
    [SerializeField]
    private Image _panel;
    [SerializeField]
    private Meter _meterPrefab;
    [Header("Visuals")]
    [SerializeField]
    private bool _showMeters = true;
    [SerializeField, Range(0, 255)]
    private float _heroColorAlpha = 180;

    private readonly Dictionary<string, float> _damageMeter = new();
    private readonly Dictionary<string, Meter> _meters = new();

    private float _battleStartTime = 0;
    private bool _battleIsActive = false;
    private float _battleTimeElapsed = 0;

    public override void SetUpInstance()
    {
        base.SetUpInstance();
        Instance = this;
    }

    protected override void SubscribeToEvents()
    {
        base.SubscribeToEvents();
        BossBase.Instance.GetBossDamagedEventDetailed().AddListener(AddToMeters);
        GameStateManager.Instance.GetStartOfBattleEvent().AddListener(OnBattleStart);
        GameStateManager.Instance.GetBattleWonOrLostEvent().AddListener(OnBattleEnd);
    }

    private void Start()
    {
        _metersParent.SetActive(_showMeters);
    }

    private void Update()
    {
        UpdateTimerUI();
    }

    private void OnValidate()
    {
        _metersParent.SetActive(_showMeters);
    }

    private void AddToMeters(MeterEventDetails details)
    {
        if (_damageMeter.ContainsKey(details.dealerName))
        {
            _damageMeter[details.dealerName] += details.amount;
        }
        else
        {
            _damageMeter.Add(details.dealerName, details.amount);
            AddMeter(details.dealerName, details.dealerColor);
        }

        UpdateMetersUI();
    }

    private void UpdateTimerUI()
    {
        if (!_battleIsActive)
            return;

        _battleTimeElapsed = Time.time - _battleStartTime;

        _timerText.text = _battleTimeElapsed.ToString("F2");
    }

    private void UpdateMetersUI()
    {
        float highestDamage = -1;
        foreach (var kvp in _damageMeter)
        {
            if (!_meters.ContainsKey(kvp.Key))
                continue;

            if(kvp.Value > highestDamage)
                highestDamage = kvp.Value;
        }

        foreach (var key in _meters.Keys)
        {
            float damage = _damageMeter[key];

            _meters[key].SetName(key);
            _meters[key].SetValueText(damage.ToString("F0") + $" ({damage/_battleTimeElapsed:F1})");
            _meters[key].SetBarFill(damage / highestDamage);
        }

        SortMeters();
    }

    private void AddMeter(string name, Color color)
    {
        Meter meter = Instantiate(_meterPrefab, _panel.transform);

        color.a = _heroColorAlpha;

        meter.SetName(name);
        meter.SetValue(0);
        meter.SetBarFill(0);
        meter.SetBarColor(color);

        _meters.Add(name, meter);
    }

    private void SortMeters(bool ascending = false)
    {
        if (_meters.Count == 0)
            return;

        IEnumerable<KeyValuePair<string, float>> sorted = ascending ? _damageMeter.OrderBy(pair => pair.Value)
            : _damageMeter.OrderByDescending(pair => pair.Value);

        int siblingIndex = 0;

        foreach (KeyValuePair<string, float> entry in sorted)
        {
            string key = entry.Key;

            if (!_meters.TryGetValue(key, out var meter))
                continue;

            meter.transform.SetSiblingIndex(siblingIndex);
            siblingIndex++;
        }
    }

    private void OnBattleStart()
    {
        _battleStartTime = Time.time;
        _battleIsActive = true;
    }

    private void OnBattleEnd()
    {
        _battleIsActive = false;
    }
}

public struct MeterEventDetails
{
    public string dealerName;
    public float amount;
    public Color dealerColor;
}
