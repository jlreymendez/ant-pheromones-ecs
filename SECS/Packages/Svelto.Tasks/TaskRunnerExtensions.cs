using System;
using Svelto.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Svelto.Tasks.Enumerators;
using Svelto.Utilities;

namespace Svelto.Tasks.ExtraLean
{
    public static class TaskRunnerExtensions
    {
        public static void RunOn<TTask, TRunner>(this TTask enumerator, TRunner runner)
            where TTask : IEnumerator where TRunner : class, IRunner<ExtraLeanSveltoTask<TTask>>
        {
            new ExtraLeanSveltoTask<TTask>().Run(runner, ref enumerator/*, false*/);
        }
        
        public static void RunOn<TRunner>(this IEnumerator enumerator, TRunner runner)
            where TRunner : class, IRunner<ExtraLeanSveltoTask<IEnumerator>>
        {
            new ExtraLeanSveltoTask<IEnumerator>().Run(runner, ref enumerator/*, false*/);
        }
    }
}

namespace Svelto.Tasks.Lean
{
    public static class TaskRunnerExtensions
    {
        public static ContinuationEnumerator RunOn<TTask, TRunner>(this TTask enumerator, TRunner runner)
            where TTask : struct, IEnumerator<TaskContract> where TRunner : class, IRunner<LeanSveltoTask<TTask>>
        {
            return new LeanSveltoTask<TTask>().Run(runner, ref enumerator/*, false*/);
        }
        
        public static ContinuationEnumerator RunOn<TRunner>(this IEnumerator<TaskContract> enumerator, TRunner runner)
            where TRunner : class, IRunner<LeanSveltoTask<IEnumerator<TaskContract>>>
        {
            return new LeanSveltoTask<IEnumerator<TaskContract>>().Run(runner, ref enumerator/*, false*/);
        }
    }
}

public static class TaskRunnerExtensions
{
    static readonly ThreadLocal<Svelto.Tasks.Lean.SyncRunner> _syncRunner = new ThreadLocal<Svelto.Tasks.Lean.SyncRunner>
        (() => new Svelto.Tasks.Lean.SyncRunner());
    
    public static TaskContract Continue<T>(this T enumerator) where T:class, IEnumerator<TaskContract> 
    {
        return new TaskContract(enumerator);
    }
    
    public static TaskContract Continue(this IEnumerator enumerator)
    {
        return new TaskContract(enumerator);
    }
    
    public static void Complete<T>(this T enumerator) where T:class, IEnumerator<TaskContract>
    {
        _syncRunner.Value.StartCoroutine(enumerator);
    }

    public static void Complete<T>(this T enumerator, int _timeout = 0) where T : IEnumerator
    {
        var quickIterations = 0;

        if (_timeout > 0)
        {
            var then  = DateTime.Now.AddMilliseconds(_timeout);
            var valid = true;

            while (enumerator.MoveNext() &&
                   (valid = DateTime.Now < then)) ThreadUtility.Wait(ref quickIterations);

            if (valid == false)
                throw new Exception("synchronous task timed out, increase time out or check if it got stuck");
        }
        else
        {
            if (_timeout == 0)
                while (enumerator.MoveNext())
                    ThreadUtility.Wait(ref quickIterations);
            else
                while (enumerator.MoveNext());
        }
    }
}

