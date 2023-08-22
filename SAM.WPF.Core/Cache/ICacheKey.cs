namespace SAM.WPF.Core
{
    public interface ICacheKey
    {
        string Key { get; }
        string FilePath { get; }

        string GetFullPath();
    }
}
