using System;

namespace Svelto.Tasks
{
    public interface IRunner: IDisposable
    {
        bool isStopping { get; }
        bool isKilled   { get; }

        void Pause();
        void Resume();
        void Stop();
        void Flush();

        uint numberOfRunningTasks { get; }
        uint numberOfQueuedTasks  { get; }
        uint numberOfProcessingTasks { get; }
    }

    public interface IRunner<T> where T:ISveltoTask
    {
        void StartCoroutine(in T task);
    }
}
