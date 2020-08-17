using Unity.Entities;
using UnityEngine;
using UInput = UnityEngine.Input;

namespace AntPheromones.Input
{
    public class SimulationInputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            for (var i = 1; i < 10; i++)
            {
                if (UInput.GetKeyUp((KeyCode)((int)KeyCode.Alpha0 + i)))
                {
                    UnityEngine.Time.timeScale = i;
                }
            }

        }
    }
}