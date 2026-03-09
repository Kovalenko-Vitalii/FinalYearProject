using UnityEngine;

public class RabbitDebugHit : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private int damage = 1;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                RabbitBrain rabbit = hit.collider.GetComponentInParent<RabbitBrain>();

                if (rabbit != null)
                {
                    rabbit.TakeDamage(damage, hit.point, gameObject);
                }
            }
        }
    }
}