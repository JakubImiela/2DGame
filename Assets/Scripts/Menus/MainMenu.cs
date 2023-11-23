using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void onClick_Settings() 
    {
        MenuManager.OpenMenu(Menu.SETTINGS, gameObject);
    }
    public void onClick_Shop()
    {
        MenuManager.OpenMenu(Menu.SHOP, gameObject);
    }
    public void onClick_NewGame(int i)
    {
        SceneController.loadScene(0);
    }
}
