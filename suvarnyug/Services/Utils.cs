using System.IO;

namespace suvarnyug.Services
{
    public class Utils
    {
        public static string GetDataFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }
    }
}
