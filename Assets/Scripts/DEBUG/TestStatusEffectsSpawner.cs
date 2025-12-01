using UnityEngine;

public class TestStatusEffectsSpawner : MonoBehaviour
{
    private void Update()
    {
        // === PAIN ===
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var effect = new PainEffect(
                duration: 999f,
                intensity: 0.7f,
                target: BodyPart.Head 
            );

            StatusEffectManager.Instance.AddEffect(effect);
            Debug.Log("Added PAIN");
        }

        // === PAINKILLER ===
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var effect = new PainkillerEffect(
                duration: 999f,
                suppression: 0.2f,
                target: BodyPart.Head
            );

            StatusEffectManager.Instance.AddEffect(effect);
            Debug.Log("Added PAINKILLER");
        }

        // === FRACTURE LEFT LEG ===
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            var effect = new FractureEffect(
                duration: 999f,
                speedMultiplier: 0.55f,
                targetPart: BodyPart.LeftLeg
            );

            StatusEffectManager.Instance.AddEffect(effect);
            Debug.Log("Added FRACTURE LeftLeg");
        }

        // === FRACTURE RIGHT LEG ===
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            var effect = new FractureEffect(
                duration: 999f,
                speedMultiplier: 0.45f,
                targetPart: BodyPart.RightLeg
            );

            StatusEffectManager.Instance.AddEffect(effect);
            Debug.Log("Added FRACTURE RightLeg");
        }

        // === BLEEDING LEFT ARM ===
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            var effect = new BleedingEffect(
                duration: 999f,
                damagePerSecond: 0.1f,
                targetPart: BodyPart.LeftArm
            );

            StatusEffectManager.Instance.AddEffect(effect);
            Debug.Log("Added BLEEDING LeftArm");
        }

        // === BLEEDING RIGHT LEG ===
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            var effect = new BleedingEffect(
                duration: 999f,
                damagePerSecond: 0.15f,
                targetPart: BodyPart.RightLeg
            );

            StatusEffectManager.Instance.AddEffect(effect);
            Debug.Log("Added BLEEDING RightLeg");
        }

        // === REMOVE ALL EFFECTS ===
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ClearAllEffects();
            Debug.Log("Cleared ALL effects");
        }
    }

    private void ClearAllEffects()
    {
        foreach (StatusEffectId id in System.Enum.GetValues(typeof(StatusEffectId)))
        {
            StatusEffectManager.Instance.RemoveEffect(id);
        }
    }
}
