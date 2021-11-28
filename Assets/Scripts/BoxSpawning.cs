using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawning : MonoBehaviour
{
    public Vector3 spawnBoxesAt;
    public float spawnDelay = 5f;
    public bool spawnAtStart = true;
    public GameObject[] boxPrefabs;

    private void Start()
    {
        int r = Random.Range(0, boxPrefabs.Length);
        Instantiate(boxPrefabs[r], spawnBoxesAt, Quaternion.identity);
    }

    private float i = 0;
    private void Update()
    {
        i += Time.deltaTime;

        if(i >= spawnDelay)
        {
            i = 0;
            int r = Random.Range(0, boxPrefabs.Length);
            Instantiate(boxPrefabs[r], spawnBoxesAt, Quaternion.identity);
        }
    }
}
