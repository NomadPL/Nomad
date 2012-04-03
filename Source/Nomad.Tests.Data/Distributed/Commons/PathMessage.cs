using System;

namespace Nomad.Tests.Data.Distributed.Commons
{
    /// <summary>
    /// Used to set CounterFile path for listener module.
    /// </summary>
    [Serializable]
    public class PathMessage : DistributableMessage
    {
        public PathMessage(string payload)
            : base(payload)
        {
        }
    }
}