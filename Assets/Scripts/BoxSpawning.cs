using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BoxSpawning : NetworkBehaviour
{
    public Vector3 spawnBoxesAt;
    public float spawnDelay = 5f;
    public bool spawnAtStart = true;
    public GameObject[] boxPrefabs;

    private void Start() {
        if(isServer && spawnAtStart) { SpawnBox(); }
    }

    private float i = 0;
    private void Update() {
        if(isServer) {
            i += Time.deltaTime;

            if(i >= spawnDelay) {
                i = 0;
                SpawnBox();
            }
        }
    }

    private void SpawnBox() {
        int r = Random.Range(0, boxPrefabs.Length);
        GameObject go = Instantiate(boxPrefabs[r], spawnBoxesAt, Quaternion.identity);
        NetworkServer.Spawn(go);
    }
}
