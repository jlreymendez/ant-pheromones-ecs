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

            //
            // todo: fix startup catchup.
            // if (UInput.GetKeyUp(KeyCode.R))
            // {
            //     World.DisposeAllWorlds();
            //     var sceneLoader = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            //     sceneLoader.completed += operation =>
            //     {
            //         DefaultWorldInitialization.Initialize("Default World", false);
            //     };
            // }

            if (UInput.GetKeyUp(KeyCode.H))
            {
                _instructionsUI.SetActive(!_instructionsUI.activeSelf);
            }
        }
    }
}