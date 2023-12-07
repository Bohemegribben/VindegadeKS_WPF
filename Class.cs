using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindegadeKS_WPF
{
    public class Class
    {
        public Quarter ClassQuarter { get; set; }
        public string ClassYear { get; set; }
        public string ClassNumber { get; set; }
        public LicenseType ClassLicenseType { get; set; }

        public string ClassName
        {
            get { return $"{ClassQuarter}{ClassYear}-{ClassNumber}"; }
            set { }
        }

        public Class(Quarter _classQuarter, string _classYear, string _classNumber, LicenseType _classLicenseType, string _className)
        {
            ClassQuarter = _classQuarter;
            ClassYear = _classYear;
            ClassNumber = _classNumber;
            ClassLicenseType = _classLicenseType;
            ClassName = _className;
        }

        public Class() : this(default, "", "", LicenseType.B, "")
        { }

        public string DisplayValue { get; set; }
    }

    public enum Quarter { Spring, Summer, Fall, Winter }
    public enum LicenseType { A1, A2, A, B }
}
