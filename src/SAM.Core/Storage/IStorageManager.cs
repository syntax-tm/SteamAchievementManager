using System.Drawing;
using System.Threading.Tasks;

namespace SAM.Core.Storage;

public interface IStorageManager
{
    string ApplicationStoragePath { get; }

    void SaveImage(string fileName, Image img, bool overwrite = true);
    Task SaveImageAsync(string fileName, Image img, bool overwrite = true);
    void SaveText(string fileName, string text, bool overwrite = true);
    Task SaveTextAsync(string fileName, string text, bool overwrite = true);
    void SaveBytes(string fileName, byte[] bytes, bool overwrite = true);
    Task SaveBytesAsync(string fileName, byte[] bytes, bool overwrite = true);
    Image GetImageFile(string fileName);
    Task<Image> GetImageFileAsync(string fileName);
    string GetTextFile(string fileName);
    Task<string> GetTextFileAsync(string fileName);
    byte[] GetBytes(string fileName);
    Task<byte[]> GetBytesAsync(string fileName);
    void CreateDirectory(string directory);
    bool FileExists(string fileName);
}
