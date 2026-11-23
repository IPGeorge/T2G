using UnityEngine;

public class SpinController : MonoBehaviour
{
    public float Speed = 30.0f; 

    void Update()
    {
        transform.Rotate(Vector3.up, Speed * Time.deltaTime);    
    }
}
