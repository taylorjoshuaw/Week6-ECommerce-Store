using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Models
{
    public class ApplicationUser : IdentityUser
    {
    }

    public static class ApplicationRoles
    {
        public static string Admin  => "Admin";
        public static string AdminNormalized => Admin.ToUpper();

        public static string Member => "Member";
        public static string MemberNormalized => Member.ToUpper();
    }
}
