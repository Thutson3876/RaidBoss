using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MetersContainer : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField]
    private Image _panel;
    [SerializeField]
    private Meter _meterPrefab;

    private readonly Dictionary<string, float> _values = new();
    private readonly Dictionary<string, Meter> _meters = new();

    public float HeroColorAlpha { get; set; } = 180;
    public float BattleTimeElapsed { get; set; } = 0;

    public Dictionary<string, float> Values => _values;

    public void AddToMeters(MeterEventDetails details)
    {
        if (_values.ContainsKey(details.dealerName))
        {
            _values[details.dealerName] += details.amount;
        }
        else
        {
            _values.Add(details.dealerName, details.amount);
            AddMeter(details.dealerName, details.dealerColor);
        }

        UpdateMetersUI();
    }

    private void UpdateMetersUI()
    {
        float highestDamage = -1;
        foreach (var kvp in _values)
        {
            if (!_meters.ContainsKey(kvp.Key))
                continue;

            if (kvp.Value > highestDamage)
                highestDamage = kvp.Value;
        }

        foreach (var key in _meters.Keys)
        {
            float damage = _values[key];

            _meters[key].SetName(key);
            _meters[key].SetValueText(damage.ToString("F0") + $" ({damage / BattleTimeElapsed:F1})");
            _meters[key].SetBarFill(damage / highestDamage);
        }

        SortMeters();
    }

    private void AddMeter(string name, Color color)
    {
        Meter meter = Instantiate(_meterPrefab, _panel.transform);

        color.a = HeroColorAlpha;

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

        IEnumerable<KeyValuePair<string, float>> sorted = ascending ? _values.OrderBy(pair => pair.Value)
            : _values.OrderByDescending(pair => pair.Value);

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
}
