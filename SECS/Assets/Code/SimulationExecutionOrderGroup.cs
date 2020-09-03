using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;

namespace AntPheromones.SECS
{
    public class SimulationExecutionOrderGroup : SortedJobifedEnginesGroup<IJobifiedEngine, SimulationEngineOrder>
    {
        public SimulationExecutionOrderGroup(FasterList<IJobifiedEngine> engines) : base(engines) { }
    }

    public struct SimulationEngineOrder : ISequenceOrder
    {
        public string[] enginesOrder => new string[] { };
    }
}