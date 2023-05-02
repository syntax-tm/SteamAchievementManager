namespace SAM.Core.Storage
{
    public interface ICacheKey
    {
        string Key { get; }
        string FilePath { get; }

        string GetFullPath();
    }
}
