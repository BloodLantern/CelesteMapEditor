using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Editor.Utils
{
    public static class Coroutine
    {
        private class Routine
        {
            public readonly IEnumerator Enumerator;

            private float timer = 0f;
            private bool timerInvalidated = true;

            public readonly Guid Guid;
            public bool Finished = false;

            private object Current => Enumerator.Current;

            public Routine(Guid guid, IEnumerator routine)
            {
                Guid = guid;
                Enumerator = routine;
            }

            private void Next()
            {
                Finished = !Enumerator.MoveNext();
                timerInvalidated = true;
            }

            public void Update(GameTime time)
            {
                if (Finished)
                    return;

                if (Current == null)
                {
                    Next();
                    return;
                }

                if (Current is not float)
                    throw new ArgumentException("Coroutines can only return null and float values");

                if (timerInvalidated)
                {
                    timer += (float) Current;
                    timerInvalidated = false;
                }

                timer -= time.GetElapsedSeconds();

                if (timer <= 0f)
                    Next();
            }
        }

        private static readonly Dictionary<Guid, Routine> runningRoutines = new();

        public static int RunningCount => runningRoutines.Count;

        /// <summary>
        /// Starts a new coroutine, assigning it a <see cref="Guid"/>. Note that the coroutine
        /// will start running next frame update.
        /// </summary>
        /// <param name="routine">The coroutine to start.</param>
        /// <returns>The newly created <see cref="Guid"/> assigned to the coroutine.</returns>
        public static Guid Start(IEnumerator routine)
        {
            Guid guid = Guid.NewGuid();
            runningRoutines.Add(guid, new(guid, routine));
            return guid;
        }

        /// <summary>
        /// Starts a coroutine using an existing coroutine <see cref="Guid"/>, stopping the existing
        /// coroutine if it is still running and assigning the guid with a newly created one.
        /// </summary>
        /// <param name="routine">The coroutine to start.</param>
        /// <param name="guid">The existing coroutine <see cref="Guid"/> that will be overriden with the new one.</param>
        public static void Start(IEnumerator routine, ref Guid guid)
        {
            if (guid != Guid.Empty && IsRunning(guid))
                Stop(guid);
            guid = Start(routine);
        }

        internal static void UpdateAll(GameTime time)
        {
            List<Guid> finishedRoutines = new();
            foreach (Routine routine in runningRoutines.Values)
            {
                routine.Update(time);
                if (routine.Finished)
                    finishedRoutines.Add(routine.Guid);
            }

            foreach (Guid guid in finishedRoutines)
                Stop(guid);
        }

        public static IEnumerator Stop(Guid guid)
        {
            if (runningRoutines.ContainsKey(guid))
            {
                IEnumerator enumerator = runningRoutines[guid].Enumerator;
                runningRoutines.Remove(guid);
                return enumerator;
            }

            return null;
        }

        public static bool IsRunning(Guid guid) => runningRoutines.ContainsKey(guid);
    }
}
