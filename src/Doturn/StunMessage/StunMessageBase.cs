namespace Doturn.StunMessage
{
    interface IStunMessage
    {
        Type Type { get; }
        byte[] ToBytes();
    }
    public abstract class StunMessageBase : IStunMessage
    {
        public abstract Type Type { get; }

        public abstract byte[] ToBytes();
    }
}