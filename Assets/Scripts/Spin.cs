using UnityEngine;

public class Spin : MonoBehaviour {
    public float rotationSpeed = 100f;

    void Update() {
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
    }
}