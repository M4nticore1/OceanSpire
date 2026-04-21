using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private int lastYear;
    private int lastMonth;
    private int lastDay;
    private int lastHour;
    private int lastMinute;
    private int lastSecond;

    public event Action<int> onYearChanged;
    public event Action<int> onMonthChanged;
    public event Action<int> onDayChanged;
    public event Action<int> onHourChanged;
    public event Action<int> onMinuteChanged;
    public event Action<int> onSecondChanged;

    private void Awake()
    {
        if (Instance) {
            Debug.Log("Duplicate TimeManager found in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static int GetCurrentSecond()
    {
        return DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
    }

    private void UpdateYear()
    {
        int year = DateTime.Now.Year;
        if (year == lastYear) return;

        lastYear = year;
        onYearChanged?.Invoke(year);
    }

    private void UpdateMonth()
    {
        int month = DateTime.Now.Month;
        if (month == lastMonth) return;

        lastMonth = month;
        onMonthChanged?.Invoke(month);
    }

    private void UpdateDay()
    {
        int day = DateTime.Now.Day;
        if (day == lastDay) return;

        lastDay = day;
        onDayChanged?.Invoke(day);
    }

    private void UpdateHour()
    {
        int hour = DateTime.Now.Hour;
        if (hour == lastHour) return;

        lastHour = hour;
        onHourChanged?.Invoke(hour);
    }

    private void UpdateMinute()
    {
        int minute = DateTime.Now.Minute;
        if (minute == lastMinute) return;

        lastMinute = minute;
        onMinuteChanged?.Invoke(minute);
    }

    private void UpdateSecond()
    {
        int second = DateTime.Now.Second;
        if (second == lastSecond) return;

        lastSecond = second;
        onSecondChanged?.Invoke(second);
    }
}