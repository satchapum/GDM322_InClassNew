using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerScript : MonoBehaviour
{
    //MainPlayerScript mainPlayer;
    // Start is called before the first frame update
    public Behaviour[] scripts;
    private Renderer[] renderers;
    void Start()
    {
        //mainPlayer = GetComponent<MainPlayerScript>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void SetPlayerState(bool state)
    {
        foreach (var script in scripts) { script.enabled = state; };
        foreach (var renderer in renderers) { renderer.enabled = state; };
    }

    private Vector3 GetRandomPos()
    {
        Vector3 ranPos = new Vector3(Random.Range(-3,3),1,Random.Range(-3,3));
        return ranPos;
    }

    public void Respawn()
    {
        RespawnServerRpc();
    }

    [ServerRpc]
    void RespawnServerRpc()
    {
        Vector3 pos = GetRandomPos();
        RespawnClientRpc(pos);
    }

    [ClientRpc]
    void RespawnClientRpc(Vector3 pos)
    {
        StartCoroutine(RespawnCoroutine(pos));
    }
    IEnumerator RespawnCoroutine(Vector3 spawnPos) 
    {
        SetPlayerState(false);
        transform.position = spawnPos;
        yield return new WaitForSeconds(3f) ;
        SetPlayerState(true);
        /*mainPlayer.enabled = false;
        transform.position = spawnPos;
        mainPlayer.enabled = true;*/
    }
}
