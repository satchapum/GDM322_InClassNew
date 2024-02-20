using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombSpawnerScript : NetworkBehaviour
{
    public GameObject bombPrefab;
    private List<GameObject> spawnedBomb = new List<GameObject>();

    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnBombServerRpc();
        }
    }

    [ServerRpc]
    void SpawnBombServerRpc()
    {
        Vector3 spawnPos = transform.position + (transform.forward * -1.5f) + (transform.up * 1.5f);
        Quaternion spawnRot = transform.rotation;
        GameObject bomb = Instantiate(bombPrefab, spawnPos, spawnRot);
        spawnedBomb.Add(bomb);
        bomb.GetComponent<BombScript>().bombSpawner = this;
        bomb.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc (RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjId)
    {
        GameObject obj = findSpawnerBomb(networkObjId);
        if (obj == null) return;
        obj.GetComponent<NetworkObject>().Despawn();
        spawnedBomb.Remove(obj);
        Destroy(obj);
    }

    private GameObject findSpawnerBomb(ulong netWorkObjId)
    {
        foreach (GameObject bomb in spawnedBomb)
        {
            ulong bombId = bomb.GetComponent<NetworkObject>().NetworkObjectId;
            if (bombId == netWorkObjId) { return bomb; }
        }
        return null;
    }
}
