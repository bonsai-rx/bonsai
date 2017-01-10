using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    struct PropertyAssignment
    {
        public string Name;
        public string Value;
        const string PropertyAssignmentSeparator = "=";

        public static PropertyAssignment Parse(string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                throw new ArgumentNullException("property");
            }

            var assignment = property.Split(new[] { PropertyAssignmentSeparator }, 2, StringSplitOptions.None);
            if (assignment.Length != 2)
            {
                throw new ArgumentException("Invalid property assignment.", "property");
            }

            PropertyAssignment result;
            result.Name = assignment[0];
            result.Value = assignment[1];
            return result;
        }
    }
}
