namespace PogoDom.Core
{
    public interface IRandomSource
    {
        uint NextUInt();
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat01();
    }
}
