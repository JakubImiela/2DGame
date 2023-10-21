using UnityEngine;

public class GPS : MonoBehaviour
{
    public enum area { A, B, C, D };
    public enum transitionType { Door, Ladder };

    public area playerCurrentArea = area.B;

    public static GPS instance;

    

    

    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        
    }


    public void setCurrentArea(area areaName)
    {
        playerCurrentArea = areaName;
    }

  


  
    
}


