using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.Tasks.DataStructures;
using Svelto.Tasks.Internal;

namespace Svelto.Tasks
{
    /// <summary>
    /// Remember, unless you are using the StandardSchedulers, nothing hold your runners. Be careful that if you
    /// don't hold a reference, they will be garbage collected even if tasks are still running
    /// </summary>
    public abstract class BaseRunner<T> : IRunner, IRunner<T> where T : ISveltoTask
    {
        public bool isStopping => _flushingOperation.stopping;
        public bool isKilled   => _flushingOperation.kill;
        public bool hasTasks   => numberOfProcessingTasks != 0;

        public uint numberOfRunningTasks    => _coroutines.count;
        public uint numberOfQueuedTasks     => _newTaskRoutines.Count;
        public uint numberOfProcessingTasks => _newTaskRoutines.Count + _coroutines.count;

        protected BaseRunner(string name, int size)
        {
            _name = name;
            _newTaskRoutines = new ThreadSafeQueue<T>(size);
            _coroutines = new FasterList<T>((uint) size);
        }

        protected BaseRunner(string name)
        {
            _name = name;
            _newTaskRoutines = new ThreadSafeQueue<T>(NUMBER_OF_INITIAL_COROUTINE);
            _coroutines = new FasterList<T>(NUMBER_OF_INITIAL_COROUTINE);
        }

        ~BaseRunner()
        {
            Console.LogWarning(this._name.FastConcat(" has been garbage collected, this could have serious" +
                                                     "consequences, are you sure you want this? "));

            Stop();
        }

        public void Flush()
        {
            Stop();
            Step();
        }

        public void Pause()
        {
            _flushingOperation.paused = true;
        }

        public void Resume()
        {
            _flushingOperation.paused = false;
        }

        public void Step()
        {
            using (var platform = new PlatformProfiler(this._name))
            {
                _processEnumerator.MoveNext(platform);
            }
        }

        /// <summary>
        /// TaskRunner doesn't stop executing tasks between scenes it's the final user responsibility to stop the tasks
        /// if needed
        /// </summary>
        public virtual void Stop()
        {
            SveltoTaskRunner<T>.StopRoutines(_flushingOperation);

            _newTaskRoutines.Clear();
        }

        public void StartCoroutine(in T task)
        {
            _newTaskRoutines.Enqueue(task);
        }

        public virtual void Dispose()
        {
            if (_newTaskRoutines == null)
            {
                Svelto.Console.LogDebugWarning($"disposing an already disposed runner?! {_name}");

                return;
            }

            Stop();

            SveltoTaskRunner<T>.KillProcess(_flushingOperation);

            GC.SuppressFinalize(this);

            _newTaskRoutines = null;
        }

        protected IProcessSveltoTasks InitializeRunner<TFlowModified>(TFlowModified modifier) where TFlowModified:IFlowModifier
        {
            _processEnumerator =
                new SveltoTaskRunner<T>.Process<TFlowModified>
                    (_newTaskRoutines, _coroutines, _flushingOperation, modifier);

            return _processEnumerator;
        }

        IProcessSveltoTasks _processEnumerator;
        ThreadSafeQueue<T> _newTaskRoutines;
        readonly FasterList<T>      _coroutines;
        readonly SveltoTaskRunner<T>.FlushingOperation _flushingOperation = new SveltoTaskRunner<T>.FlushingOperation();

        readonly string _name;

        const int NUMBER_OF_INITIAL_COROUTINE = 3;
    }
}