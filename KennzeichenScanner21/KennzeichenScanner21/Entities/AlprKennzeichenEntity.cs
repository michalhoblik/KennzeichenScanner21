using System;
using System.Collections.Generic;

namespace KennzeichenScanner21.Entities
{
    public class AlprKennzeichenEntity
    {
        public List<AlprKennzeichen> FoundLicenses { get; set; }
        public bool ErrorOccured { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class AlprKennzeichen
    {
        public string LicensePlate { get; set; }
        public double Confidence { get; set; }
    }
}
