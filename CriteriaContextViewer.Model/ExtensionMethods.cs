using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CriteriaContextViewer.Model
{
    public static class ExtensionMethods
    {
        public static bool NotYetImplemented(this Enum value)
        {
            IEnumerable<NYIAttribute> attributes =
                Enum.GetValues(typeof(NYIAttribute))
                    .Cast<Enum>()
                    .Where(value.HasFlag)
                    .Select(v => typeof(NYIAttribute).GetField(v.ToString()))
                    .SelectMany(f => f.GetCustomAttributes(typeof(NYIAttribute), false))
                    .Cast<NYIAttribute>();
            return attributes.Any();
        }

        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}
