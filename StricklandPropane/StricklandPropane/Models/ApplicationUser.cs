using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public State HomeState { get; set; }
        public GrillingPreference GrillingPreference { get; set; }
    }

    public static class ApplicationRoles
    {
        public static string Admin => "Admin";
        public static string AdminNormalized => Admin.ToUpper();

        public static string Member => "Member";
        public static string MemberNormalized => Member.ToUpper();
    }

    public enum GrillingPreference : Int32
    {
        Propane,
        Charcoal,
        Wood,
        LaserBeams
    }

    public enum State : Int32
    {
        Alabama,
        Alaska,
        Arizona,
        Arkansas,
        California,
        Colorado,
        Connecticut,
        Delaware,
        Florida,
        Georgia,
        Hawaii,
        Idaho,
        Illinois,
        Indiana,
        Iowa,
        Kansas,
        Kentucky,
        Louisiana,
        Maine,
        Maryland,
        Massachusetts,
        Michigan,
        Minnesota,
        Mississippi,
        Missouri,
        Montana,
        Nebraska,
        Nevada,
        [Display(Name = "New Hampshire")] NewHampshire,
        [Display(Name = "New Jersey")] NewJersey,
        [Display(Name = "New Mexico")] NewMexico,
        [Display(Name = "New York")] NewYork,
        [Display(Name = "North Carolina")] NorthCarolina,
        [Display(Name = "North Dakota")] NorthDakota,
        Ohio,
        Oklahoma,
        Oregon,
        Pennsylvania,
        [Display(Name = "Rhode Island")] RhodeIsland,
        [Display(Name = "South Carolina")] SouthCarolina,
        [Display(Name = "South Dakota")] SouthDakota,
        Tennessee,
        Texas,
        Utah,
        Vermont,
        Virginia,
        Washington,
        [Display(Name = "West Virginia")] WestVirginia,
        Wisconsin,
        Wyoming,
    }
}
