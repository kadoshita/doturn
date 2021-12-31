namespace Doturn.StunAttribute
{
    public interface IStunAttribute
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