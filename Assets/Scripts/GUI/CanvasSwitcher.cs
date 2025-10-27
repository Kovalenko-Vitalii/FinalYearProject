using System.Collections.Generic;
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Main menus")]
    public GameObject escapeMenu;
    public GameObject tabMenu;

    [Header("Submenus")]
    public SubMenuEntry[] subMenuEntries;

    private Dictionary<SubMenuType, GameObject> subMenusDict;
    private Stack<GameObject> menuStack = new Stack<GameObject>();

    public enum SubMenuType
    {
        Inventory,
        Character,
        GearSelection
    }

    [System.Serializable]
    public struct SubMenuEntry
    {
        public SubMenuType type;
        public GameObject menu;
    }

    private void Awake()
    {
        subMenusDict = new Dictionary<SubMenuType, GameObject>();
        foreach (var entry in subMenuEntries)
        {
            if (entry.menu != null)
                subMenusDict[entry.type] = entry.menu;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu(tabMenu);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuStack.Count > 0)
                CloseLastMenu();
            ToggleMenu(escapeMenu);
        }      
    }

    public void ToggleMenu(GameObject menu)
    {
        if (menu == null) return;

        if (menu.activeSelf)
            CloseMenu(menu);
        else
            OpenMenu(menu);
    }

    public void OpenMenu(GameObject menu)
    {
        if (menu == null) return;

        menu.SetActive(true);
        menuStack.Push(menu);
        UpdatePlayerState();
    }

    public void CloseMenu(GameObject menu)
    {
        if (menu == null) return;

        if (menuStack.Contains(menu))
        {
            menu.SetActive(false);

            if (menu == tabMenu)
            {
                foreach (var submenu in subMenusDict.Values)
                {
                    submenu.SetActive(false);
                    RemoveFromStack(submenu);
                }
            }
            RemoveFromStack(menu);
        }
        else
        {
            menu.SetActive(false);
        }
        UpdatePlayerState();
    }

    public void CloseLastMenu()
    {
        if (menuStack.Count > 0)
        {
            var last = menuStack.Pop();
            menuStack.Push(last);
            return;
        }
        UpdatePlayerState();
    }

    public void OpenTabSubMenu(SubMenuType type)
    {
        if (subMenusDict.TryGetValue(type, out var menu))
            OpenMenu(menu);
    }

    public void SwitchTabSubMenu(SubMenuType type)
    {
        if (!subMenusDict.TryGetValue(type, out var targetMenu))
            return;

        foreach (var submenu in subMenusDict.Values)
        {
            submenu.SetActive(false);
            RemoveFromStack(submenu);
        }
        OpenMenu(targetMenu);
    }

    private void RemoveFromStack(GameObject menu)
    {
        if (!menuStack.Contains(menu)) return;

        var temp = new Stack<GameObject>();
        while (menuStack.Count > 0)
        {
            var top = menuStack.Pop();
            if (top != menu) temp.Push(top);
            else break;
        }
        while (temp.Count > 0) menuStack.Push(temp.Pop());
    }

    private void UpdatePlayerState()
    {
        bool menuOpen = menuStack.Count > 0;
        PauseManager.Instance.SetPlayerControl(!menuOpen);
    }

}
