using System.Collections.Generic;
using UnityEngine;

public class TabContainer : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string id;
        public GameObject panel;
    }

    [SerializeField] private List<Tab> tabs;
    private string _current;

    public void Show(string id)
    {
        _current = id;
        foreach (var t in tabs)
            t.panel.SetActive(t.id == id);
    }

    public void Switch(string id)
    {
        if (_current == id)
            return;

        foreach (var t in tabs)
        {
            if (t.id == _current)
                t.panel.SetActive(false);
        }

        foreach (var t in tabs)
        {
            if (t.id == id)
            {
                t.panel.SetActive(true);
                _current = id;
                break;
            }
        }
    }

}
