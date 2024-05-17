namespace SAM.Core.Storage
{
    public interface ICacheKey
    {
        string Key { get; }
        string FilePath { get; }

        uint? DaysValid { get; }
        bool HasExpiration { get; }

        string GetFullPath();
    }
}
