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
        public string DocStuCPR { get; set; }
        public DateTime DocStartDate { get; set; }
        public DateTime DocEndDate { get; set; }
        public DocTypeEnum DocType { get; set; }

        public byte[] DocFile { get; set; }

        public Documentation(int _docId, string _stuCPR,DateTime _docStartDate, DateTime _docEndDate, DocTypeEnum  _docType)
        {
            DocId = _docId;
            DocStuCPR = _stuCPR;
            DocStartDate = _docStartDate;
            DocEndDate = _docEndDate;
            DocType = _docType;
        }

        public Documentation() : this(0,"", default, default, default)
        { }

        public enum DocTypeEnum { LægeErklæring, SpecialErklæring, FørsteHjælp}
    }
}
