namespace Nomad.Messages.Distributed
{
    /// <summary>
    ///     Message which is send when event aggregator is being 
    /// deteached from the currently active distributed messaging system.
    /// </summary>
    public class NomadDetachingMessage : NomadDistributedMessage
    {
        public NomadDetachingMessage(string message) : base(message)
        {
        }

        public override string ToString()
        {
            return "Distributed Detaching Message";
        }
    }
}