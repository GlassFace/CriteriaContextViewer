using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CriteriaContextViewer.Model
{
    public static class ExtensionMethods
    {
        public static bool NotYetImplemented(this Enum enumerator)
        {
            IEnumerable<NYIAttribute> attributes =
                Enum.GetValues(typeof(NYIAttribute))
                    .Cast<Enum>()
                    .Where(enumerator.HasFlag)
                    .Select(v => typeof(NYIAttribute).GetField(v.ToString()))
                    .SelectMany(f => f.GetCustomAttributes(typeof(NYIAttribute), false))
                    .Cast<NYIAttribute>();
            return attributes.Any();
        }
    }
}
