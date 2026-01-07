using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KURSOVAYA_RABOTA.models;

namespace KURSOVAYA_RABOTA.models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public decimal Balance { get; set; }
        public byte[] AvatarImage { get; set; }
    }
}
