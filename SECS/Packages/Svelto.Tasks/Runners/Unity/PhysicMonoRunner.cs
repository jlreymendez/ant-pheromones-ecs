#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using Svelto.Tasks.FlowModifiers;
using Svelto.Tasks.Internal;
using Svelto.Tasks.Unity.Internal;

namespace Svelto.Tasks
{
    namespace Lean.Unity
    {
        public class PhysicMonoRunner:PhysicMonoRunner<IEnumerator<TaskContract>>
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class PhysicMonoRunner<T> : Svelto.Tasks.Unity.PhysicMonoRunner<LeanSveltoTask<T>> where T : IEnumerator<TaskContract>
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace ExtraLean.Unity
    {
        public class PhysicMonoRunner:PhysicMonoRunner<IEnumerator>
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }

        public class PhysicMonoRunner<T> : Svelto.Tasks.Unity.PhysicMonoRunner<ExtraLeanSveltoTask<T>> where T : IEnumerator
        {
            public PhysicMonoRunner(string name) : base(name)
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder)
            {
            }
        }
    }

    namespace Unity
    {
        public class PhysicMonoRunner<T> : PhysicMonoRunner<T, StandardRunningInfo> where T : ISveltoTask
        {
            public PhysicMonoRunner(string name) : base(name, 0, new StandardRunningInfo())
            {
            }
            public PhysicMonoRunner(string name, uint runningOrder) : base(name, runningOrder, new StandardRunningInfo())
            {
            }
        }

        public class PhysicMonoRunner<T, TFlowModifier> : BaseRunner<T> where T : ISveltoTask
                                                                        where TFlowModifier : IFlowModifier
        {
            public PhysicMonoRunner(string name, uint runningOrder, TFlowModifier modifier) : base(name)
            {
                modifier.runnerName = name;

                var _processEnumerator = InitializeRunner(modifier);

                UnityCoroutineRunner.StartPhysicCoroutine(_processEnumerator, runningOrder);
            }
        }
    }
}
#endif
