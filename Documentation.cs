using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindegadeKS_WPF
{
    public class Documentation
    {
        public int DocId { get; set; }
        public DateOnly DocStartDate { get; set; }
        public DateOnly DocEndDate { get; set; }
        public string DocType { get; set; }

        public Documentation(int _docId, DateOnly _docStartDate, DateOnly _docEndDate, string _docType)
        {
            DocId = _docId;
            DocStartDate = _docStartDate;
            DocEndDate = _docEndDate;
            DocType = _docType;
        }

        public Documentation() : this(0, default, default, "")
        { }
    }
}
