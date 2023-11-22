using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindegadeKS_WPF
{
    public class Instructor
    {
        public int InstId { get; set; }
        public string InstFirstName { get; set; }
        public string InstLastName { get; set; }
        public string InstPhone { get; set; }
        public string InstEmail { get; set; }

        public Instructor(int _instId, string _instFirstName, string _instLastName, string _instPhone, string _instEmail)
        {
            InstId = _instId;
            InstFirstName = _instFirstName;
            InstLastName = _instLastName;
            InstPhone = _instPhone;
            InstEmail = _instEmail;
        }

        public Instructor() : this(0, "", "", "", "")
        { }
    }
}