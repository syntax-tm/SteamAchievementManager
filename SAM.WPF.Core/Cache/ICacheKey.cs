namespace SAM.WPF.Core
{
    public interface ICacheKey
    {
        string Key { get; }
        string Path { get; }

        string GetFullPath();
    }
}
