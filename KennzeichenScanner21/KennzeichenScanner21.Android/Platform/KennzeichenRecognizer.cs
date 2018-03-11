using KennzeichenScanner21.Droid.Platform;
using KennzeichenScanner21.Entities;
using KennzeichenScanner21.Interface;

[assembly: Xamarin.Forms.Dependency(typeof(KennzeichenRecognizer))]
namespace KennzeichenScanner21.Droid.Platform
{
    public class KennzeichenRecognizer : IKennzeichenRecognizer
    {
        public AlprKennzeichenEntity Recognize(string imagePath, string country, string region)
        {
            var androidDataDir = MainActivity.Instance.ApplicationInfo.DataDir;
            var openAlprConfigFile = androidDataDir + "/runtime_data/openalpr.conf";

            var plateRecog = new OpenAlprScanner(androidDataDir, openAlprConfigFile, country, region);
            var result = plateRecog.Recognize(imagePath);
            return result;
        }
    }
}