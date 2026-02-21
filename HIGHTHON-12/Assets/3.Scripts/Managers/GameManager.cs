using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject); return; 
        }
        I = this;

        DontDestroyOnLoad(gameObject);
    }
}