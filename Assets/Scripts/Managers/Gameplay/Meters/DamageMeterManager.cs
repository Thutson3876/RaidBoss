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
    private MetersContainer _damageMeters;
    [SerializeField]
    private MetersContainer _healingMeters;
    [SerializeField]
    private MetersContainer _staggerMeters;
    [Space]
    [SerializeField]
    private GameObject _metersParent;
    [SerializeField]
    private TMP_Text _timerText;
    [Header("Visuals")]
    [SerializeField]
    private bool _showMeters = true;
    [SerializeField, Range(0, 255)]
    private float _heroColorAlpha = 180;
    [Header("Logging")]
    [SerializeField]
    private bool _logMeters = true;

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

        BossBase.Instance.GetBossDamagedEventDetailed().AddListener(AddToDamageMeters);
        HeroesManager.Instance.GetOnHeroHealedEventDetailed().AddListener(AddToHealingMeters);
        BossBase.Instance.GetBossStaggerDealtEventDetailed().AddListener(AddToStaggerMeters);

        GameStateManager.Instance.GetStartOfBattleEvent().AddListener(OnBattleStart);
        GameStateManager.Instance.GetBattleWonEvent().AddListener(OnBattleWon);
        GameStateManager.Instance.GetBattleLostEvent().AddListener(OnBattleLost);
    }

    private void Start()
    {
        _metersParent.SetActive(_showMeters);

        _damageMeters.HeroColorAlpha = _heroColorAlpha;
        _healingMeters.HeroColorAlpha = _heroColorAlpha;
        _staggerMeters.HeroColorAlpha = _heroColorAlpha;
    }

    private void Update()
    {
        UpdateTimerUI();
    }

    private void OnValidate()
    {
        _metersParent.SetActive(_showMeters);
    }

    private void AddToDamageMeters(MeterEventDetails details)
    {
        _damageMeters.AddToMeters(details);
    }

    private void AddToHealingMeters(MeterEventDetails details)
    {
        _healingMeters.AddToMeters(details);
    }

    private void AddToStaggerMeters(MeterEventDetails details)
    {
        _staggerMeters.AddToMeters(details);
    }

    private void UpdateTimerUI()
    {
        if (!_battleIsActive)
            return;

        _battleTimeElapsed = Time.time - _battleStartTime;

        _damageMeters.BattleTimeElapsed = _battleTimeElapsed;
        _healingMeters.BattleTimeElapsed = _battleTimeElapsed;
        _staggerMeters.BattleTimeElapsed = _battleTimeElapsed;

        _timerText.text = _battleTimeElapsed.ToString("F2");
    }

    private void OnBattleStart()
    {
        _battleStartTime = Time.time;
        _battleIsActive = true;
    }

    private void OnBattleWon()
    {
        OnBattleEnd(true);
    }

    private void OnBattleLost()
    {
        OnBattleEnd(false);
    }

    private void OnBattleEnd(bool won)
    {
        _battleIsActive = false;

        if (_logMeters)
        {
            int difficulty = SelectionManager.Instance.GetSelectedDifficultyID() + SelectionManager.Instance.GetMythicPlusLevel();
            List<MissionModifierSO> modifiers = SelectionManager.Instance.GetCurrentMissionModifiers();
            List<int> modifierIDs = modifiers.Select(m => m.GetModifierID()).ToList();

            MeterLogger.LogMeters(won, difficulty, modifierIDs, _battleTimeElapsed, _damageMeters.Values, _staggerMeters.Values, _healingMeters.Values);
        }
            
    }
}

public struct MeterEventDetails
{
    public string dealerName;
    public float amount;
    public Color dealerColor;
}
