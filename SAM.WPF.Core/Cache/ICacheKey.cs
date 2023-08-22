namespace SAM.WPF.Core.Cache
{
    public interface ICacheKey
    {
        string Key { get; }
        string Path { get; }

        string GetFullPath();
    }
}
