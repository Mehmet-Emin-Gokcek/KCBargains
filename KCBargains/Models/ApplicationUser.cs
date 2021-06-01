using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UsernameChangeLimit { get; set; } = 10;
        public byte[] ProfilePicture { get; set; }
        public string AdminId{ get; set; }
        public ApplicationUser Admin { get; set; }
        public string TimeLog { get; set; }

        public ApplicationUser() {

            TimeLog = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
        }

    }
}
