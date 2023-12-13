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

        public string Setup;

        public Appointment(int _apmtId, DateTime _apmtDate)
        {
            ApmtId = _apmtId;
            ApmtDate = _apmtDate;
        }

        public Appointment() : this(0, default)
        { }
    }
}
