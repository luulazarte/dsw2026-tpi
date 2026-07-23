using Dsw2026Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class User : EntityBase
    {
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public User(string email, string passwordHash, UserRole role, Guid? id = null) : base(id)
        {
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }
        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.Now;
        }

        public void UpdateEmail(string email)
        {
            Email = email;
            UpdatedAt = DateTime.Now;
        }
    }
}
