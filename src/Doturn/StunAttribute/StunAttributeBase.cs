namespace Doturn.StunAttribute
{
    interface IStunAttribute
    {
        Type Type { get; }
        byte[] ToByte();
    }
    public abstract class StunAttributeBase : IStunAttribute
    {
        public abstract Type Type { get; }

        public abstract byte[] ToByte();
    }
}