﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VindegadeKS_WPF
{
    public class Student
    {
        public string StuCPR { get; set; }
        public string StuFirstName { get; set; }
        public string StuLastName { get; set; }
        public string StuPhone { get; set; }
        public string StuEmail { get; set; }
        public string Setup { get; set; }

        //Used by DocumentationPage
        public string StuFullName
        {
            get
            {
                return $"{StuFirstName} {StuLastName}";
            }
        }

        // Constructor
        public Student(string _stuCPR, string _stuFirstName, string _stuLastName, string _stuPhone, string _stuEmail, string _setup)
        {
            StuCPR = _stuCPR;
            StuFirstName = _stuFirstName;
            StuLastName = _stuLastName;
            StuPhone = _stuPhone;
            StuEmail = _stuEmail;
            Setup = _setup;
        }
        public Student() : this("", "", "", "", "", "")
        { }
    }
}
