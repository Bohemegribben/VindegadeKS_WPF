using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindegadeKS_WPF
{
    public class Lesson
    {
        public int LesId { get; set; }
        public string LesName { get; set; }
        public string LesType { get; set; }
        public DateOnly LesDate { get; set; }
        public string LesDescription { get; set; }

        public Lesson(int _lesId, string _lesName, string _lesType, DateOnly _lesDate, string _lesDescription)
        {
            LesId = _lesId;
            LesName = _lesName;
            LesType = _lesType;
            LesDate = _lesDate;
            LesDescription = _lesDescription;
        }
        public Lesson() : this(0, "", "", default, "")
        { }
    }
}