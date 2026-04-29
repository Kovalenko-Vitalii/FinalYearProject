using UnityEngine;

// This class represents damageble obstacle that manages physical object that have HP that can be destroyed by damage
[RequireComponent(typeof(DamageableObstacle))]
[RequireComponent(typeof(InteractableOutline))]
public class ObstacleOutline : MonoBehaviour
{
    [SerializeField] private DamageableObstacle obstacle;
    [SerializeField] private InteractableOutline outline;

    [SerializeField] private Color fullHpColor = Color.white;
    [SerializeField] private Color midHpColor = Color.yellow;
    [SerializeField] private Color lowHpColor = Color.red;

    [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.33f;

    private void Reset()
    {
        obstacle = GetComponent<DamageableObstacle>();
        outline = GetComponent<InteractableOutline>();
    }

    private void Awake()
    {
        if (obstacle == null)
            obstacle = GetComponent<DamageableObstacle>();

        if (outline == null)
            outline = GetComponent<InteractableOutline>();
    }

    public void Show()
    {
        outline.Show(GetColor());
    }

    public void Hide()
    {
        outline.Hide();
    }

    private Color GetColor()
    {
        float hp01 = obstacle.Hp01;

        if (hp01 >= 1f)
            return fullHpColor;

        if (hp01 > lowThreshold)
            return midHpColor;

        return lowHpColor;
    }
}