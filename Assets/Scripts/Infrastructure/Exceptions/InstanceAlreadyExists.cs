using System;

namespace Assets.Scripts.Infrastructure.Exceptions
{
    /// <summary>
    /// Exception thrown when a singleton object is reinstantiated.
    /// </summary>
    /// <typeparam name="T">The type of the singleton.</typeparam>
    public class InstanceAlreadyExists<T> : Exception
    {
        public T ExistingInstance { get; private set; }

        public InstanceAlreadyExists(string message, T existingInstance) : base(message)
        {
            ExistingInstance = existingInstance;
        }
    }
}