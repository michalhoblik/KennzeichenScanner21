using System;

namespace KennzeichenScanner21.Entities
{
    public class KennzeichenEntity
    {
        public string LicensePlate { get; set; }
        public double Confidence { get; set; }
        public string Region { get; set; }
        public string Template { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
