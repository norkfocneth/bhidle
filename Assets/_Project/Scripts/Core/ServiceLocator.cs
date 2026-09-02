using System;
using System.Collections.Generic;
using UnityEngine;

namespace TERRAGRAV.Core
{
    /// <summary>
    /// Lightweight, type-safe, reflection-free Service Locator.
    /// Provides explicit registration and retrieval without hidden object creation.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service instance. If a service of type T already exists, logs a warning and overwrites it.
        /// </summary>
        public static void Register<T>(T service)
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceLocator] Attempted to register null service of type {typeof(T).Name}!");
                return;
            }

            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already registered. Overwriting with new instance.");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
            }
        }

        /// <summary>
        /// Unregisters a service instance of type T.
        /// </summary>
        public static void Unregister<T>()
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Retrieves the registered service of type T. Returns null and logs error if missing.
        /// </summary>
        public static T Get<T>()
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            Debug.LogError($"[ServiceLocator] Requested service of type {type.Name} is not registered!");
            return default;
        }

        /// <summary>
        /// Safely queries whether a service of type T is registered without error logging.
        /// </summary>
        public static bool TryGet<T>(out T service)
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object rawService))
            {
                service = (T)rawService;
                return true;
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
