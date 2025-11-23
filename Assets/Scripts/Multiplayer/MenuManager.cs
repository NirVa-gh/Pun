using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private Menu[] _menus;
    private void Awake()
    {
        Instance = this;
    }
    public void OpenMenu(string menuName)
    {
        foreach (var menuElem in _menus)
        {
            if (menuElem.menuName == menuName)
            {
                OpenMenu(menuElem);
            }
            else if (menuElem.isOpen)
            {
                CloseMenu(menuElem);
            }
        }
    }

    public void OpenMenu(Menu menu)
    {
        foreach(var menuElem in _menus)
        {
            if (menuElem.isOpen)
            {
                CloseMenu(menuElem);
            }
        }
        menu.Open();
    }

    private void CloseMenu(Menu menu)
    {
        menu.Close();
    }

    public void QuitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
