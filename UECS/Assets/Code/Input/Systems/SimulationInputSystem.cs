using AntPheromones.Ants;
using AntPheromones.Obstacles.Systems;
using Code.Pheromones;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UInput = UnityEngine.Input;

namespace AntPheromones.Input
{
    public class SimulationInputSystem : SystemBase
    {
        GameObject _instructionsUI;

        protected override void OnStartRunning()
        {
            _instructionsUI = GameObject.FindWithTag("UI");
        }

        protected override void OnUpdate()
        {
            for (var i = 1; i < 10; i++)
            {
                if (UInput.GetKeyUp((KeyCode)((int)KeyCode.Alpha0 + i)))
                {
                    UnityEngine.Time.timeScale = i;
                }
            }

            if (UInput.GetKeyUp(KeyCode.R))
            {
                EntityManager.DestroyEntity(EntityManager.GetAllEntities());
                World.GetOrCreateSystem<SpawningColonySystem>().Enabled = true;
                World.GetOrCreateSystem<SpawningFoodSystem>().Enabled = true;
                World.GetOrCreateSystem<SpawningObstaclesSystem>().Enabled = true;
                World.GetOrCreateSystem<AntsSpawningSystem>().Enabled = true;
                World.GetOrCreateSystem<PheromoneSpawningSystem>().Enabled = true;
            }

            if (UInput.GetKeyUp(KeyCode.H))
            {
                _instructionsUI.SetActive(!_instructionsUI.activeSelf);
            }
        }
    }
}