using UnityEngine;

public class DateWeatherManager : MonoBehaviour, IPlayerTick
{
    public static DateWeatherManager Instance { get; private set; }
    [SerializeField] int dayDurationSeconds = 12000;
    [SerializeField] float currentDaySeconds = 0;
    [SerializeField] int currentDay = 1;
    [SerializeField] float dayDuration;

    private void Start()
    {
        PlayerTickSystem.Instance?.Register(this);
    }


    private void OnEnable()
    {
        if (PlayerTickSystem.Instance != null)
            PlayerTickSystem.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (PlayerTickSystem.Instance != null)
            PlayerTickSystem.Instance.Unregister(this);
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void Tick(float dt)
    {
        TickSeconds(dt);
    }

    private void TickSeconds(float dt)
    {
        Debug.Log("SECOND");
        if(currentDaySeconds >= dayDurationSeconds)
        {
            currentDaySeconds = 0;
            currentDay++;
        } else
        {
            currentDaySeconds++;
        }
    }

}
