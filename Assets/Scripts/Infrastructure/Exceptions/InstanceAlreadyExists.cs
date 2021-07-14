using System;

namespace BaseBuilder_Reloaded.Scripts.Infrastructure.Exceptions
{
    public class InstanceAlreadyExists<T> : Exception
    {
        public T ExistingInstance { get; private set; }

        public InstanceAlreadyExists(string message, T existingInstance) : base(message)
        {
            ExistingInstance = existingInstance;
        }
    }
}