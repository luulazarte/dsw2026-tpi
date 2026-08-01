using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient : EntityBase
    {
        protected Patient() { 
        }
        public Guid UserId { get; private set; }
        public String Dni {  get; private set; }
        public String FullName { get; private set; }

        public Patient(Guid userId, string dni, String fullName, Guid? id = null) : base(id)
        {
            UserId = userId;
            Dni = dni;
            FullName = fullName;
        }
    }
}
