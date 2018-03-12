using KennzeichenScanner21.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KennzeichenScanner21.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainScanPage : TabbedPage
    {
        public ObservableCollection<KennzeichenEntity> AlprKennzeichenEntities = new ObservableCollection<KennzeichenEntity>();

        public MainScanPage ()
        {
            InitializeComponent();
            AlprKennzeichenView.ItemsSource = AlprKennzeichenEntities;
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

                        AddkennzeichenToList(result);
                    }
                }
            }
        }

        private void AddkennzeichenToList(AlprKennzeichenEntity alprKennzeichenEntity)
        {
            var kennzeichen = alprKennzeichenEntity.FoundLicenses.FirstOrDefault();
            if (kennzeichen != null)
            {
                AlprKennzeichenEntities.Add(new KennzeichenEntity()
                {
                    LicensePlate = kennzeichen.LicensePlate,
                    Confidence = kennzeichen.Confidence,
                    TimeStamp = alprKennzeichenEntity.TimeStamp
                });
            }
        }
    }
}