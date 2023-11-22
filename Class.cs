using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindegadeKS_WPF
{
    public class Class
    {
        private string className;
        public string ClassQuarter { get; set; }
        public string ClassYear { get; set; }
        public string ClassNumber { get; set; }
        public string ClassLicenceType { get; set; }

        public string ClassName
        {
            get { return className; }
            set
            {
                value = $"{ClassQuarter ?? "x"}{ClassYear ?? "xx"}-{ClassNumber ?? "x"}";
                className = value;
            }
        }

        public Class(string _className, string _classQuarter, string _classYear, string _classNumber, string _classLicenceType)
        {
            ClassName = _className;
            ClassQuarter = _classQuarter;
            ClassYear = _classYear;
            ClassNumber = _classNumber;
            ClassLicenceType = _classLicenceType;
        }

        public Class() : this("", "", "", "", "")
        { }
    }
}
