using UnityEngine;

public class ObjectInterface : MonoBehaviour
{
    public float Scale
    {
        set { transform.localScale *= value; }
        get { return transform.localScale.x; }
    }

    public Vector3 Scale3D
    {
        set { transform.localScale = value; }
        get { return transform.localScale; }
    }

    public Vector3 Position
    {
        set { transform.localPosition = value; }
        get { return transform.localPosition; }
    }
    public Vector3 Rotation3D
    {
        set { transform.localRotation = Quaternion.Euler(value); }
        get { return transform.localRotation.eulerAngles; }
    }

    public Color Color
    {
        set 
        {
            var material = GetComponent<MeshRenderer>().material;
            material.color = value;
        }
    }



}
