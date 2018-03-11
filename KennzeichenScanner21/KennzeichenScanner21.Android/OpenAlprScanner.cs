using KennzeichenScanner21.Entities;
using OpenALPR_Xamarin.Android_Library;
using System;
using System.Collections.Generic;

namespace KennzeichenScanner21.Droid
{
    public class OpenAlprScanner
    {
        private OpenALPR _openALPRInstance;

        public OpenAlprScanner(string dataDirPath, string openAlprConfigFilePath, string country = "eu", string region = "")
        {
            _openALPRInstance = new OpenALPR(MainActivity.Instance, dataDirPath, openAlprConfigFilePath, country, region);
        }

        public AlprKennzeichenEntity Recognize(string imagePath)
        {
            var result = new AlprKennzeichenEntity() { TimeStamp = DateTime.Now };

            try
            {
                if (System.IO.File.Exists(imagePath)) 
                {
                    var recognitionResults = _openALPRInstance.Recognize(imagePath, 5);

                    if (!recognitionResults.DidErrorOccur)
                    {
                        result.FoundLicenses = new List<AlprKennzeichen>();
                        foreach (var rr in recognitionResults.FoundLicensePlates)
                        {
                            var foundPlate = new AlprKennzeichen() { LicensePlate = rr.BestLicensePlate, Confidence = rr.Confidence };
                            result.FoundLicenses.Add(foundPlate);
                        }
                    }
                    else
                    {
                        result.ErrorOccured = true;
                        result.ErrorMessage = recognitionResults.Error.Message + ", " + recognitionResults.Error.Stacktrace;
                    }
                }
                else
                {
                    result.ErrorOccured = true;
                    result.ErrorMessage = "Image-Datei wurde nicht gefunden";
                }
            }
            catch (Exception ex)
            {
                result.ErrorOccured = true;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }
}