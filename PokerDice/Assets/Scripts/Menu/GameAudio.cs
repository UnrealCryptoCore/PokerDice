using UnityEngine;

public class GameAudio : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("dont destrouy");
    }

}
