using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StricklandPropane.Models.Policies
{
    public static class ApplicationPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string MemberOnly = "MemberOnly";
        public const string TexansOnly = "TexansOnly";
        public const string PropaneAdvocatesOnly = "PropaneAdvocatesOnly";

        /// <summary>
        /// Creates an IEnumerable of strings for every policy in the
        /// ApplicationPolicies static class using CLR reflection.
        /// </summary>
        /// <returns>An IEnumerable of strings for each constant declared
        /// in the ApplicationPolicies class.</returns>
        public static IEnumerable<string> ToEnumerable() =>
            typeof(ApplicationPolicies)
                .GetFields(BindingFlags.Public | BindingFlags.Static |
                           BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy)
                .Where(pi => pi.IsLiteral && !pi.IsInitOnly)
                .Select(pi => pi.GetRawConstantValue())
                .Cast<string>();
    }
}
