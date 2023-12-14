using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindegadeKS_WPF
{
    public class Appointment
    {
        public int ApmtId { get; set; }
        public DateTime ApmtDate { get; set; }
        public int FK_InstId { get; set; }
        public int FK_LesId { get; set; }
        public string FK_ClassName { get; set; }

        public string Setup;

        public Appointment(int _apmtId, DateTime _apmtDate, int _fK_InstId, int _fK_LesId, string _fK_ClassName)
        {
            ApmtId = _apmtId;
            ApmtDate = _apmtDate;
            FK_InstId = _fK_InstId;
            FK_LesId = _fK_LesId;
            FK_ClassName = _fK_ClassName;
        }

        public Appointment() : this(0, default, 0, 0, "")
        { }
    }
}
