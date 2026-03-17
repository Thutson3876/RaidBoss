using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Meter : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _nameText;
    [SerializeField]
    private TMP_Text _valueText;
    [SerializeField]
    private Image _bar;

    public void SetName(string name)
    {
        _nameText.text = name;
    }

    public void SetValue(float val, int decimalPlaces = 0)
    {
        _valueText.text = val.ToString($"F{decimalPlaces}");
    }

    public void SetValueText(string text)
    {
        _valueText.text = text;
    }

    public void SetBarFill(float fill)
    {
        _bar.fillAmount = fill;
    }

    public void SetBarColor(Color color)
    {
        _bar.color = color;
    }
}
