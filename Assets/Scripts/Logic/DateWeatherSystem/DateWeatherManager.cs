using System;
using UnityEngine;

public class DateWeatherManager : MonoBehaviour, IPlayerTick, ISaveable
{
    public static DateWeatherManager Instance { get; private set; }

    [SerializeField] private string id = "DateWeatherManager";

    [Header("Calendar")]
    [SerializeField] private int currentDay = 1;
    
    [Header("Day Length (real time)")]
    [SerializeField] private float realMinutesPerDay = 20f;
    public bool IsPaused { get; private set; }

    [Header("Start Time")]
    [SerializeField, Range(0f, 24f)] private float startTimeHours = 9f;

    [Header("Sun Time in hours")]
    [SerializeField, Range(0f, 24f)] private float sunriseTimeHours = 6.5f;

    [Tooltip("Sunset time in hours")]
    [SerializeField, Range(0f, 24f)] private float sunsetTimeHours = 18.5f;

    const float MinutesPerDay = 1440f;
    const float HoursPerDay = 24f;
    const float MinutesPerHour = 60f;

    [Header("Sun")]
    [SerializeField] private Light sun;
    [SerializeField] private Vector3 sunAxis = Vector3.right;
    [SerializeField] private float sunAngleOffset = -90f;

    [Tooltip("Controls intensity over the day (0..1 time -> intensity multiplier)")]
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField, Tooltip("Current time in minutes (debug).")]
    private float currentMinutes;

    public int Day => currentDay;

    public float Time01 => currentMinutes / MinutesPerDay;          // 0..1
    public float TimeHours => currentMinutes / MinutesPerHour;         // 0..24
    public bool IsDaytime => IsInTimeWindow(TimeHours, sunriseTimeHours, sunsetTimeHours);

    public float GameMinutesPerSecond => MinutesPerDay / (realMinutesPerDay * MinutesPerHour);

    public string SaveId => id;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // initialize start time
        currentMinutes = Mathf.Repeat(startTimeHours * MinutesPerHour, MinutesPerDay);
    }

    private void OnEnable()
    {
        PlayerTickSystem.Instance?.Register(this);
        LevelContextRegistry.Instance.OnContextChanged += OnLevelContextChanged;
    }

    private void OnDisable()
    {
        PlayerTickSystem.Instance?.Unregister(this);
        LevelContextRegistry.Instance.OnContextChanged -= OnLevelContextChanged;
    }

    private void OnLevelContextChanged(LevelContext ctx)
    {
        sun = ctx ? ctx.DirectionalLight : null;
    }

    public void Tick(float dt)
    {
        AdvanceTime(dt);
        ApplySun();
    }

    // In this method we count time
    private void AdvanceTime(float dt)
    {
        if (!IsPaused)
        {
            currentMinutes += dt * GameMinutesPerSecond;

            if (currentMinutes >= MinutesPerDay)
            {
                currentMinutes -= MinutesPerDay;
                currentDay++;
            }
        }
    }

    // In this method we rotate sun object
    private void ApplySun()
    {
        if (!sun) return;

        float t = Time01;

        // rotation
        float angle = t * 360f + sunAngleOffset;
        sun.transform.rotation = Quaternion.AngleAxis(angle, sunAxis);

        // intensity based on curves and day/night window
        float dayFactor = EvaluateDaylight(TimeHours, sunriseTimeHours, sunsetTimeHours);
        float intensityFactor = intensityCurve.Evaluate(t);

        sun.intensity = Mathf.Max(0f, dayFactor) * intensityFactor;

        RenderSettings.ambientIntensity = Mathf.Lerp(0.2f, 1.0f, dayFactor);
    }

    private static float EvaluateDaylight(float timeH, float sunriseH, float sunsetH)
    {
        // handle wrap (if sunset < sunrise)
        bool isDay = IsInTimeWindow(timeH, sunriseH, sunsetH);
        if (!isDay) return 0f;

        // normalized inside day window
        float dayLength = HoursUntil(sunriseH, sunsetH);
        float sinceSunrise = HoursUntil(sunriseH, timeH);
        float x = (dayLength <= 0.001f) ? 0f : Mathf.Clamp01(sinceSunrise / dayLength);

        // smooth hill (0 at edges, 1 in middle)
        return Mathf.Sin(x * Mathf.PI);
    }

    private static bool IsInTimeWindow(float value, float start, float end)
    {
        value = Mathf.Repeat(value, HoursPerDay);
        start = Mathf.Repeat(start, HoursPerDay);
        end = Mathf.Repeat(end, HoursPerDay);

        if (start <= end) return value >= start && value < end;
        // wraps midnight
        return value >= start || value < end;
    }

    private static float HoursUntil(float from, float to)
    {
        float d = Mathf.Repeat(to - from, HoursPerDay);
        return d < 0 ? d + HoursPerDay : d;
    }


    // handy api methods
    public void SetTimeHours(float hours)
    {
        currentMinutes = Mathf.Repeat(hours * MinutesPerHour, MinutesPerDay);
        ApplySun();
    }

    public void SetTimeHM(int hours, int minutes)
    {
        currentMinutes = Mathf.Repeat(hours * MinutesPerHour + minutes, MinutesPerDay);
        ApplySun();
    }

    public string GetTimeString()
    {
        int total = Mathf.FloorToInt(currentMinutes);
        int h = (total / (int)MinutesPerHour) % (int)HoursPerDay;
        int m = total % (int)MinutesPerHour;
        return $"{h:00}:{m:00}";
    }

    public void AddTime(int hours, int minutes)
    {
        currentMinutes += Mathf.Repeat(hours * MinutesPerHour + minutes, MinutesPerDay);
        ApplySun();
    }

    // For save
    [Serializable]
    public struct DateWeatherSave
    {
        public int day;
        public float minutes;
    }

    public object CaptureState()
    {
        var data = new DateWeatherSave
        {
            day = currentDay,
            minutes = currentMinutes
        };
        return data;
    }

    public void RestoreState(object state)
    {
        if (state is not DateWeatherSave save) return;

        currentDay = save.day;
        currentMinutes = save.minutes;

        ApplySun();
    }
}
