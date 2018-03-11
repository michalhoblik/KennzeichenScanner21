using KennzeichenScanner21.Droid.Platform;
using KennzeichenScanner21.Interface;

[assembly: Xamarin.Forms.Dependency(typeof(CommonPlatform))]
namespace KennzeichenScanner21.Droid.Platform
{
    public class CommonPlatform : ICommonPlatform
    {
        public string GetDataDirPath()
        {
            return MainActivity.Instance.ApplicationInfo.DataDir;
        }
    }
}