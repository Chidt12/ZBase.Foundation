using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZBase.Foundation.Singletons
{
    /// <summary>
    /// <para>Designed for decoupling Singleton pattern and user classes.</para>
    /// <para>Usage: <see cref="Singleton"/>.Of&lt;MyClass&gt;()</para>
    /// </summary>
    public static partial class Singleton
    {
        private static readonly List<Action> s_resetCallbacks = new();

        public static T Of<T>() where T : class, new()
            => Single<T>.GetInstance(() => new T());

        public static T Of<T>(Func<T> instantiator) where T : class
        {
            if (instantiator == null)
                throw new ArgumentNullException(nameof(instantiator));

            return Single<T>.GetInstance(instantiator);
        }

        /// <seealso href="https://docs.unity3d.com/Manual/DomainReloading.html"/>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            lock (s_resetCallbacks)
            {
                foreach (var reset in s_resetCallbacks)
                {
                    reset();
                }
            }
        }

        private static class Single<T> where T : class
        {
            private static T s_instance;

            static Single()
            {
                lock (s_resetCallbacks)
                {
                    s_resetCallbacks.Add(static () => s_instance = null);
                }
            }

            public static T GetInstance(Func<T> instantiator)
            {
                if (s_instance == null)
                {
                    s_instance = instantiator();
                }

                return s_instance;
            }
        }
    }
}
