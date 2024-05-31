using System.Dynamic;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HelixAPI.Helpers
{
    public class ConvertionHelpers
    {
        public static ExpandoObject? CreateExpandoObject<T>(T Object, List<string> selectedFields)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var field in selectedFields)
            {
                var propertyInfo = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null)
                    expando[field] = propertyInfo.GetValue(Object);
            }

            return expando as ExpandoObject;
        }
    }
}
