using KennzeichenScanner21.Entities;

namespace KennzeichenScanner21.Interface
{
    public interface IKennzeichenRecognizer
    {
        AlprKennzeichenEntity Recognize(string imagePath, string country, string region = "");
    }
}
