using System;
using UnityEngine;

/// <summary>
/// Static, PlayerPrefs-backed currency balance shared across scenes/sessions.
/// Kept static rather than a MonoBehaviour singleton since currency has no
/// per-frame behaviour - it just needs to persist and notify listeners.
/// </summary>
public static class CurrencyManager
{
    private const string CurrencyKey = "PlayerCurrency";

    public static event Action<int> OnCurrencyChanged;

    public static int GetBalance()
    {
        return PlayerPrefs.GetInt(CurrencyKey, 0);
    }

    public static void AddCurrency(int amount)
    {
        if (amount <= 0) return;

        int newBalance = GetBalance() + amount;
        PlayerPrefs.SetInt(CurrencyKey, newBalance);
        PlayerPrefs.Save();

        OnCurrencyChanged?.Invoke(newBalance);
    }

    /// <summary>
    /// Attempts to deduct the given amount. Returns false (and leaves the
    /// balance untouched) if funds are insufficient.
    /// </summary>
    public static bool TrySpend(int amount)
    {
        if (amount < 0) return false;
        if (amount == 0) return true;

        int balance = GetBalance();
        if (balance < amount) return false;

        int newBalance = balance - amount;
        PlayerPrefs.SetInt(CurrencyKey, newBalance);
        PlayerPrefs.Save();

        OnCurrencyChanged?.Invoke(newBalance);
        return true;
    }

    /// <summary>Editor/debug helper - do not call from shipping gameplay code.</summary>
    public static void ResetBalance(int amount = 0)
    {
        PlayerPrefs.SetInt(CurrencyKey, amount);
        PlayerPrefs.Save();
        OnCurrencyChanged?.Invoke(amount);
    }
}