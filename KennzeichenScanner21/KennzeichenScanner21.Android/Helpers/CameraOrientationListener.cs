using Android.App;
using Android.Views;

namespace KennzeichenScanner21.Droid.Helpers
{
    public class CameraOrientationListener : OrientationEventListener
    {
        public const int ORIENTATION_UNKNOWN =0;

        private int _currentNormalizedOrientation;
        private int _rememberedNormalizedOrientation;

        public CameraOrientationListener(Activity activity) : base(activity)
        {            
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (orientation != ORIENTATION_UNKNOWN)
            {
                _currentNormalizedOrientation = normalize(orientation);
            }
        }

        private int normalize(int degrees)
        {
            if (degrees > 315 || degrees <= 45)
            {
                return 0;
            }

            if (degrees > 45 && degrees <= 135)
            {
                return 90;
            }

            if (degrees > 135 && degrees <= 225)
            {
                return 180;
            }

            if (degrees > 225 && degrees <= 315)
            {
                return 270;
            }

            throw new Java.Lang.RuntimeException("The physics as we know them are no more. Watch out for anomalies.");
        }


        public void rememberOrientation()
        {
            _rememberedNormalizedOrientation = _currentNormalizedOrientation;
        }

        public int getRememberedOrientation()
        {
            return _rememberedNormalizedOrientation;
        }
    }
}