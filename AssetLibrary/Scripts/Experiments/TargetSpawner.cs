using System.Collections;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject Target;
    public int Width = 10;
    public int Depth = 6;

    void Start()
    {
        if(Target == null)
        {
            Target = transform.GetChild(0).gameObject; 
        }
        Target.SetActive(false);
        StartCoroutine(CheckSpawnTarget());
    }

    IEnumerator CheckSpawnTarget()
    {
        while(true)
        {
            yield return new WaitForSeconds(3.0f);
            if(!Target.activeSelf)
            {
                Target.transform.localPosition = new Vector3(Random.Range(-1.0f, 1.0f) * Width, 0.0f, Random.Range(-1.0f, 1.0f) * Depth);
                Target.SetActive(true);
            }
        }
    }

}
