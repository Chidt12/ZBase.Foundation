using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZBase.Foundation.Singletons
{
    /// <summary>
    /// <para>Designed for decoupling Singleton pattern and <see cref="MonoBehaviour"/>-based classes.</para>
    /// <para>Usage: <see cref="SingleBehaviour"/>.Of&lt;MyMonoBehaviour&gt;()</para>
    /// </summary>
    public static partial class SingleBehaviour
    {
        public enum Lifetime
        {
            /// <summary>
            /// The singleton <see cref="MonoBehaviour"/> should only exist
            /// in the current scene.
            /// </summary>
            /// <remarks>
            /// This option will NOT affect any existing <see cref="GameObject"/>.
            /// </remarks>
            Default = 0,

            /// <summary>
            /// The singleton <see cref="MonoBehaviour"/> should only exist
            /// in the current scene.
            /// </summary>
            /// <remarks>
            /// This option will NOT affect any existing <see cref="GameObject"/>.
            /// </remarks>
            SingleScene = 1,

            /// <summary>
            /// The singleton <see cref="MonoBehaviour"/> should exist
            /// through every scene.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This option will only invoke
            /// <see cref="Object"/>.<see cref="Object.DontDestroyOnLoad(Object)"/>
            /// on the newly created <see cref="GameObject"/>.
            /// </para>
            /// <para>
            /// This option will NOT affect any existing <see cref="GameObject"/>.
            /// </para>
            /// </remarks>
            EveryScenes = 2,
        }

        private static readonly List<Action> s_resetCallbacks = new();

        public static T Of<T>(Lifetime lifetime = Lifetime.Default) where T : MonoBehaviour
            => Single<T>.GetInstance(lifetime);

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

        private static class Single<T> where T : MonoBehaviour
        {
            private static readonly object s_lock = new();
            private static T s_instance;

            static Single()
            {
                lock (s_resetCallbacks)
                {
                    s_resetCallbacks.Add(static () => s_instance = null);
                }
            }

            public static T GetInstance(Lifetime lifetime)
            {
                if (s_instance == false)
                {
                    lock (s_lock)
                    {
                        s_instance = UnityEngine.Object.FindAnyObjectByType<T>();

                        if (s_instance == false)
                        {
                            var gameObject = new GameObject();

                            if (lifetime == Lifetime.EveryScenes)
                            {
                                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                            }

                            s_instance = gameObject.AddComponent<T>();
                        }

                        s_instance.gameObject.name = $"[{nameof(Singleton)}] {typeof(T).Name}";
                    }
                }

                return s_instance;
            }
        }
    }
}
