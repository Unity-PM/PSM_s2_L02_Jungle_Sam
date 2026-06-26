using UnityEngine;
using UnityEngine.SceneManagement;

namespace JungleSam.Rendering
{
    public static class TerrainGrassWindDisabler
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            DisableGrassWindInLoadedScene();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            DisableGrassWindInLoadedScene();
        }

        private static void DisableGrassWindInLoadedScene()
        {
            Terrain[] terrains = Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Terrain terrain in terrains)
            {
                DisableGrassWind(terrain);
            }
        }

        private static void DisableGrassWind(Terrain terrain)
        {
            if (terrain == null || terrain.terrainData == null)
            {
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            terrainData.wavingGrassAmount = 0f;
            terrainData.wavingGrassSpeed = 0f;
            terrainData.wavingGrassStrength = 0f;

        }
    }
}
