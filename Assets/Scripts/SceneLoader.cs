using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider _loadingBar;
    [SerializeField] private Transform _spawnpoint;

    public void LoadSceneWithProgress(string sceneName, GameObject playerPrefab)
    {
        StartCoroutine(LoadSceneAsync(sceneName, playerPrefab));
    }

    private IEnumerator LoadSceneAsync(string sceneName, GameObject playerPrefab)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            _loadingBar.value = progress;

            if (asyncLoad.progress >= 0.9f)
            {
                _loadingBar.value = 1f;
                asyncLoad.allowSceneActivation = true;

                if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
                {
                    SpawnPlayer(playerPrefab);
                }
            }

            yield return null;
        }
    }

    private void SpawnPlayer(GameObject playerPrefab)
    {
        // Lógica para verificar se o jogador já está na sala antes de instanciar
        bool playerExists = false;
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            if (player.Value.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerExists = true;
                break;
            }
        }

        if (!playerExists)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, _spawnpoint.position, Quaternion.identity);
        }
    }
}
