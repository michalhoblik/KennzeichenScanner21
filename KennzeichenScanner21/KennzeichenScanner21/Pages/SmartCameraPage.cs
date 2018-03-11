using KennzeichenScanner21.Entities;
using Xamarin.Forms;

namespace KennzeichenScanner21.Pages
{
    public class LicenseFoundEventArgs
    {
        public LicenseFoundEventArgs(AlprKennzeichenEntity entity) { Entity = entity; }
        public AlprKennzeichenEntity Entity { get; private set; } // readonly
    }

    public class SmartCameraPage : Page
    {
        public delegate void LicenseFoundEventHandler(object sender, LicenseFoundEventArgs e);
        public event LicenseFoundEventHandler LicenseFound;

        public void RaiseLicenseFoundEvent(AlprKennzeichenEntity entity)
        {
            var eventHandler = LicenseFound;
            eventHandler?.Invoke(this, new LicenseFoundEventArgs(entity));
        }
    }
}
