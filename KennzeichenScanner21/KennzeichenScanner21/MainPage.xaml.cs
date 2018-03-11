using KennzeichenScanner21.Pages;
using System;
using System.Linq;
using Xamarin.Forms;

namespace KennzeichenScanner21
{
    public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
            AddHandlers();
		}

        private void AddHandlers()
        {
            btnRecognize.Clicked += BtnRecognize_Clicked;
        }

        private async void BtnRecognize_Clicked(object sender, EventArgs e)
        {
            var scanPage = new SmartCameraPage();
            scanPage.LicenseFound += ScanPage_LicenseFound;
            await Navigation.PushModalAsync(scanPage);
        }

        private void ScanPage_LicenseFound(object sender, LicenseFoundEventArgs e)
        {
            var result = e.Entity;
            if (result != null)
            {
                if (result.ErrorOccured)
                {
                    lblResult.Text = $"Fehler: {result.ErrorMessage}";
                }
                else
                {
                    if (result.FoundLicenses != null)
                    {
                        if (result.FoundLicenses.Count == 1)
                        {
                            var kennzeichen = result.FoundLicenses.FirstOrDefault();
                            lblResult.Text = $"Es wurde ein Kennzeichen gefunden:\nZeitstempel: {result.TimeStamp}\n---------------------\nKennzeichen: {kennzeichen.LicensePlate}\nZuversichtlichkeit: {kennzeichen.Confidence}%";
                        }
                        else
                        {
                            var resultText = $"Es wurden mehrere Kennzeichen gefunden:\nZeitstempel: {result.TimeStamp}\n---------------------\n";
                            foreach (var kennzeichen in result.FoundLicenses)
                            {
                                resultText += $"Kennzeichen: {kennzeichen.LicensePlate}\nZuversichtlichkeit: {kennzeichen.Confidence}%\n---------------------\n";
                            }

                            lblResult.Text = resultText;
                        }
                    }            
                }
            }
        }
    }
}
