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
        Debug.Log("Triggered 1");
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Triggered 2");
            other.gameObject.transform.position = _conn._spawnPoint.position;
        }
    }
}
