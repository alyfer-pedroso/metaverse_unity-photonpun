using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaterialColor : MonoBehaviour
{
    private SkinnedMeshRenderer _meshRenderer;

    public void SetPlayerColors(Color color)
    {
        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>() ?? GetComponent<SkinnedMeshRenderer>();

        if (_meshRenderer != null)
        {
            Material[] materials = _meshRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetColor("_BaseColor", color);
            }
            _meshRenderer.materials = materials;
        }
    }
}
