using System.Drawing;

namespace SAM.Core.Storage;

public interface IStorageManager
{
    void SaveImage(string fileName, Image img, bool overwrite = true);
    void SaveText(string fileName, string text, bool overwrite = true);
    void SaveBytes(string fileName, byte[] bytes, bool overwrite = true);
    Image GetImageFile(string fileName);
    string GetTextFile(string fileName);
    byte[] GetBytes(string fileName);
    void CreateDirectory(string directory);
    bool FileExists(string fileName);
}
