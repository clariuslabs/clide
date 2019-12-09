using System;

namespace Clide
{
    public interface IStartableMetadata
    {
        string Context { get; }

        double Order { get; }
    }
}
