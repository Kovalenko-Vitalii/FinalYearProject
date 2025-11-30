using UnityEngine;

public class TestStatusEffectsSpawner : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var effect = new BleedingEffect(
                duration: 999f,
                damagePerSecond: 0.1f,
                targetPart: BodyPart.LeftArm
            );

            StatusEffectManager.Instance.AddEffect(effect);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var effect = new BleedingEffect(
                duration: 999f,
                damagePerSecond: 0.1f,
                targetPart: BodyPart.RightLeg
            );

            StatusEffectManager.Instance.AddEffect(effect);
        }
    }
}
