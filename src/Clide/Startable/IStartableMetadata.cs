using System;

namespace Clide
{
    public interface IStartableMetadata
    {
        string Context { get; }

        Guid ContextGuid { get; }
    }
}
