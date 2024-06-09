using HelixAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HelixAPI.Helpers
{
    public class QueryHelpers
    {
        public static IQueryable<T> ProcessQueryFilters<T>(QueryDto queryDto, DbSet<T> DataTable) where T : class
        {
            ArgumentNullException.ThrowIfNull(DataTable);

            var query = DataTable.AsQueryable();

            foreach (var filter in queryDto.Filters)
            {
                if (!GetPropertyType<T>(out Type? propertyType, filter))
                    continue;

                // Handle enum properties
                if (propertyType.IsEnum)
                {
                    if (!CheckEnumFilter(ref query, propertyType, filter))
                        continue;
                }
                else if (propertyType == typeof(DateTime))
                {
                    if (CheckDateFilter(ref query, filter))
                        continue;
                }
                else if (propertyType == typeof(int))
                {
                    if (CheckIntFilter(ref query, filter))
                        continue;
                }
                else if (propertyType == typeof(Guid))
                {
                    if (CheckGuidFilter(ref query, filter))
                        continue;
                }
                else
                    CheckStringFilter(ref query, filter);
            }

            // Sorting
            CheckSortOrder(ref query, queryDto);

            // Pagination
            CheckPagination(ref query, queryDto);

            return query;
        }

        private static bool GetPropertyType<T>(out Type? PropertyType, FilterDto filter)
        {
            PropertyType = null;

            var propertyInfo = typeof(T).GetProperty(filter.Property);
            if (propertyInfo == null)
                return false;

            PropertyType = propertyInfo.PropertyType;

            return true;
        }

        private static bool CheckEnumFilter<T>(ref IQueryable<T> query, Type PropertyType, FilterDto filter)
        {
            if (!Enum.TryParse(PropertyType, filter.Value.ToString(), true, out var enumValue))
                return false;

            query = filter.Operation.ToLower() switch
            {
                "equals" => query.Where($"{filter.Property} == @0", enumValue),
                "notequals" => query.Where($"{filter.Property} != @0", enumValue),
                _ => throw new NotSupportedException($"Operation '{filter.Operation}' is not supported for enum types."),
            };

            return true;
        }

        private static bool CheckDateFilter<T>(ref IQueryable<T> query, FilterDto filter) 
        {
            if (!DateTime.TryParse(filter.Value.ToString(), out var dateTimeValue))
                return false;

            query = filter.Operation.ToLower() switch
            {
                "equals" => query.Where($"{filter.Property} == @0", dateTimeValue),
                "greaterthan" => query.Where($"{filter.Property} > @0", dateTimeValue),
                "lessthan" => query.Where($"{filter.Property} < @0", dateTimeValue),
                _ => throw new NotSupportedException($"Operation '{filter.Operation}' is not supported for DateTime types."),
            };

            return true;
        }

        private static bool CheckIntFilter<T>(ref IQueryable<T> query, FilterDto filter)
        {
            if (!int.TryParse(filter.Value.ToString(), out var intValue))
                return false;

            query = filter.Operation.ToLower() switch
            {
                "equals" => query.Where($"{filter.Property} == @0", intValue),
                "greaterthan" => query.Where($"{filter.Property} > @0", intValue),
                "lessthan" => query.Where($"{filter.Property} < @0", intValue),
                "notequals" => query.Where($"{filter.Property} != @0", intValue),
                _ => throw new NotSupportedException($"Operation '{filter.Operation}' is not supported for int types."),
            };

            return true;
        }

        private static bool CheckGuidFilter<T>(ref IQueryable<T> query, FilterDto filter)
        {
            if (!Guid.TryParse(filter.Value.ToString(), out var guidValue))
                return false;

            query = filter.Operation.ToLower() switch
            {
                "equals" => query.Where($"{filter.Property} == @0", guidValue),
                "notequals" => query.Where($"{filter.Property} != @0", guidValue),
                _ => throw new NotSupportedException($"Operation '{filter.Operation}' is not supported for Guid types."),
            };

            return true;
        }

        private static void CheckStringFilter<T>(ref IQueryable<T> query, FilterDto filter) =>
            // Handle non-enum properties
            query = filter.Operation.ToLower() switch
            {
                "contains" => query.Where($"{filter.Property}.Contains(@0)", filter.Value),
                "equals" => query.Where($"{filter.Property} == @0", filter.Value),
                "doesnotcontain" => query.Where($"!{filter.Property}.Contains(@0)", filter.Value),
                "notequals" => query.Where($"{filter.Property} != @0", filter.Value),
                _ => throw new NotSupportedException($"Operation '{filter.Operation}' is not supported."),
            };

        private static void CheckSortOrder<T>(ref IQueryable<T> query, QueryDto queryDto)
        {
            query = queryDto.SortOrder.Equals("desc", StringComparison.CurrentCultureIgnoreCase)
                ? query.OrderBy($"{queryDto.SortBy} desc")
                : query.OrderBy($"{queryDto.SortBy} asc");
        }
        
        private static void CheckPagination<T>(ref IQueryable<T> query, QueryDto queryDto)
        {
            query = query.Skip(queryDto.Offset).Take(queryDto.Size);
        }

        public static IEnumerable<ExpandoObject?> FilterFields<T>(List<T> TList, QueryDto queryDto)
        {
            var selectedFields = queryDto.Fields.Split(',').Select(f => f.Trim()).ToList();
            var response = TList.Select(u => CreateExpandoObject(u, selectedFields));

            return response;
        }

        private static ExpandoObject? CreateExpandoObject<T>(T Object, List<string> selectedFields)
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
