using KennzeichenScanner21.Interface;
using Xamarin.Forms;

namespace KennzeichenScanner21
{
    public partial class App : Application
	{
        public static ICommonPlatform CommonPlatform;
        public static IKennzeichenRecognizer KennzeichenRecognizer;

        public App ()
		{
			InitializeComponent();

            CommonPlatform = DependencyService.Get<ICommonPlatform>();
            KennzeichenRecognizer = DependencyService.Get<IKennzeichenRecognizer>();

            //MainPage = new KennzeichenScanner21.MainPage();
            MainPage = new Pages.MainScanPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
