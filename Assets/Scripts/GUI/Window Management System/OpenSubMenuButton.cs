using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenSubMenuButton : MonoBehaviour
{
    [SerializeField] private TabContainer container;
    [SerializeField] private string tabId;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (container != null) container.Show(tabId);
        });
    }
}
