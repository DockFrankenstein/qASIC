using System;
using System.Collections;
using System.Collections.Generic;

namespace qASIC
{
    /// <summary>List containing registered objects.</summary>
    public class qRegisteredObjects : IEnumerable<object>
    {
        public qRegisteredObjects() { }
        public qRegisteredObjects(qRegisteredObjects other)
        {
            objects = other.objects;
        }

        List<object> objects = new List<object>();

        public event Action<object> OnObjectRegistered;
        public event Action<object> OnObjectDeregistered;

        /// <summary>Registers an object.</summary>
        /// <param name="obj">Object to register.</param>
        public void Register(object obj)
        {
            objects.Add(obj);
            OnObjectRegistered?.Invoke(obj);
        }

        /// <summary>Registers multiple objects.</summary>
        /// <param name="enumerable">Objects to register.</param>
        public void RegisterMultiple(IEnumerable<object> enumerable)
        {
            foreach (var item in enumerable)
                Register(item);
        }

        /// <summary>Deregisters an object.</summary>
        /// <param name="obj">Object to deregister.</param>
        public void Deregister(object obj)
        {
            objects.Remove(obj);
            OnObjectDeregistered?.Invoke(obj);
        }

        /// <summary>Deregisters multiple objects.</summary>
        /// <param name="enumrable">Objects to deregister.</param>
        public void DeregisterMultiple(IEnumerable<object> enumrable)
        {
            foreach (var item in enumrable)
                Deregister(item);
        }

        /// <summary>Copies elements of another list of registered objects and subscribes to it's future changes. If the other list is null, it will be ignored.</summary>
        /// <param name="other">Other list to keep in sync with.</param>
        public void SyncWithOther(qRegisteredObjects other)
        {
            if (other == null) return;
            other.OnObjectRegistered += Register;
            other.OnObjectDeregistered += Deregister;
            RegisterMultiple(other);
        }

        /// <summary>Unsubscribes from another lists changes. If the other list is null, it will be ignored.</summary>
        /// <param name="other">Other list to stop being in sync with.</param>
        public void StopSyncingWithOther(qRegisteredObjects other)
        {
            if (other == null) return;
            other.OnObjectRegistered -= Register;
            other.OnObjectDeregistered -= Deregister;
            DeregisterMultiple(other);
        }

        public IEnumerator<object> GetEnumerator() =>
            objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
