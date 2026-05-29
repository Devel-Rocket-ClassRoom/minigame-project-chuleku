using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public enum Scope
{
    Permanent,
    ThisRound,
    Timed,
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private int permanentBonus;
    private int thisRoundBonus;
    public TextMeshProUGUI upgradeText;

    private class TimedEntry
    {
        public int amount;
        public float remaining;
    }
    private readonly List<TimedEntry> timedEntries = new();
    private int timedBonusCache;

    public int GlobalAttackBonus => permanentBonus + thisRoundBonus + timedBonusCache;

    public event Action OnUpgradeChanged;

    void Awake()
    {
        Instance = this;
        upgradeText.text = GlobalAttackBonus.ToString();
    }

    public void AddAttackBonus(int amount, Scope scope, float seconds = 0f)
    {
        switch (scope)
        {
            case Scope.Permanent:
                permanentBonus += amount;
                break;
            case Scope.ThisRound:
                thisRoundBonus += amount;
                break;
            case Scope.Timed:
                if (seconds <= 0f) return;
                timedEntries.Add(new TimedEntry { amount = amount, remaining = seconds });
                RecalcTimedCache();
                break;
        }
        OnUpgradeChanged?.Invoke();
        upgradeText.text = GlobalAttackBonus.ToString();
    }

    // CardGameManager.EndRound() 끝에서 호출. ThisRound 보정치를 만료시킨다.
    public void OnRoundEnded()
    {
        if (thisRoundBonus == 0) return;
        thisRoundBonus = 0;
        OnUpgradeChanged?.Invoke();
        upgradeText.text = GlobalAttackBonus.ToString();
    }

    void Update()
    {
        if (timedEntries.Count == 0) return;

        float dt = Time.deltaTime;
        bool changed = false;
        for (int i = timedEntries.Count - 1; i >= 0; i--)
        {
            timedEntries[i].remaining -= dt;
            if (timedEntries[i].remaining <= 0f)
            {
                timedEntries.RemoveAt(i);
                changed = true;
            }
        }
        if (changed)
        {
            RecalcTimedCache();
            OnUpgradeChanged?.Invoke();
            upgradeText.text = GlobalAttackBonus.ToString();
        }
    }

    private void RecalcTimedCache()
    {
        int sum = 0;
        for (int i = 0; i < timedEntries.Count; i++) sum += timedEntries[i].amount;
        timedBonusCache = sum;
    }
}
