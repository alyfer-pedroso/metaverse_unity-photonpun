using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidSystem : MonoBehaviour
{
    [Header("Network Manager")]
    [SerializeField] private Conn _conn;

    void Start()
    {
        _conn = GameObject.FindWithTag("network_manager").GetComponent<Conn>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            other.gameObject.transform.position = _conn._spawnPoint.position;
    }
}
