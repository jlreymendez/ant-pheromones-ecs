using System;
using System.Collections;
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Internal;
using Svelto.Tasks;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;
using Unity.Entities;
using Unity.Jobs;

namespace AntPheromones.SECS
{
    public class SimulationCompositionRoot : ICompositionRoot
    {
        static readonly PhysicMonoRunner TickScheduler = new PhysicMonoRunner("FixedTick");
        static World _world;

        EnginesRoot _enginesRoot;
        FasterList<IJobifiedEngine> _enginesToTick;
        SimpleEntitiesSubmissionScheduler _submissionScheduler;


        public void OnContextInitialized<T>(T contextHolder)
        {
            // Create engine root.
            _submissionScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_submissionScheduler);

            // Initialize UECS.
            _world = new World("SveltoWorld");
            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(_world, systems);
            World.DefaultGameObjectInjectionWorld = _world;

            var syncGroup = new SyncSveltoToUECSGroup();
            _world.AddSystem(syncGroup);
            AddTickEngine(syncGroup);

            var uecsTickGroup = new PureUECSSystemsGroup(_world);
            AddTickEngine(uecsTickGroup);

            // Initialize SECS
            var entityFactory = _enginesRoot.GenerateEntityFactory();
        }

        public void OnContextDestroyed()
        {
            TickScheduler.Dispose();
            _enginesRoot?.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void OnContextCreated<T>(T contextHolder) { }

        void AddCallbackEngine(IReactEngine engine)
        {
            _enginesRoot.AddEngine(engine);
        }

        void AddTickEngine(IJobifiedEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            _enginesToTick.Add(engine);
        }

        void AddSveltoToUnityEngine<T>(T engine) where T : SyncSveltoToUECSEngine
        {
            _world.AddSystem(engine);
            _enginesRoot.AddEngine(engine);

            var syncGroup = _world.GetExistingSystem<SyncSveltoToUECSGroup>();
            syncGroup.AddSystemToUpdateList(engine);
        }

        void Start(FasterList<IJobifiedEngine> engines)
        {
            MainThreadTick(engines).RunOn(TickScheduler);
        }

        IEnumerator MainThreadTick(FasterList<IJobifiedEngine> engines)
        {
            var orderGroup = new SimulationExecutionOrderGroup(engines);
            JobHandle jobs = default;

            while (true)
            {
                jobs.Complete();
                _submissionScheduler.SubmitEntities();
                jobs = orderGroup.Execute(jobs);
                yield return Yield.It;
            }
        }
    }
}