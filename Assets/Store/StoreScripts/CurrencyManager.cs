using System;
using UnityEngine;


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

 
    public static void ResetBalance(int amount = 0)
    {
        PlayerPrefs.SetInt(CurrencyKey, amount);
        PlayerPrefs.Save();
        OnCurrencyChanged?.Invoke(amount);
    }
}