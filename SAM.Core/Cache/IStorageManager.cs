using System.Drawing;

namespace SAM.Core;

public interface IStorageManager
{
    void SaveImage(string fileName, Image img, bool overwrite = true);
    void SaveText(string fileName, string text, bool overwrite = true);
    Image GetImageFile(string fileName);
    string GetTextFile(string fileName);
    void CreateDirectory(string directory);
    bool FileExists(string fileName);
}
