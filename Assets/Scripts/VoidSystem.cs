using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidSystem : MonoBehaviour
{
    [Header("Network Manager")]
    [SerializeField] private NetworkManager _networkManager;

    void Start()
    {
        _networkManager = GameObject.FindWithTag("network_manager").GetComponent<NetworkManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            other.gameObject.transform.position = _networkManager._spawnPoint.position;
    }
}
