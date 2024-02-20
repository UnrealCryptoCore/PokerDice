using UnityEngine;

public class DiceRole : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(300, -20, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
