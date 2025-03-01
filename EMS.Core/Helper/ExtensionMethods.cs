using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EMS.Core.Helpers
{
    public static class ExtensionMethods
    {
        public static string ToBase64(this string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        public static string FromBase64(this string encodedString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }

        public static string ToJson<T>(this T self, bool ignoreNullValues = false)
        {
            try { return JsonConvert.SerializeObject(self, Formatting.Indented, ignoreNullValues ? JsonSettingsWithoutNullValues : JsonSettings); }
            catch { return ""; }
        }

        public static T FromJson<T>(this string json, bool ignoreNullValues = false)
        {
            try { return JsonConvert.DeserializeObject<T>(json ?? string.Empty, ignoreNullValues ? JsonSettingsWithoutNullValues : JsonSettings); }
            catch { return default; }
        }

        private static JsonSerializerSettings JsonSettings => new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            Error = (se, ev) => { ev.ErrorContext.Handled = true; }
        };

        private static JsonSerializerSettings JsonSettingsWithoutNullValues => new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Error = (se, ev) => { ev.ErrorContext.Handled = true; }
        };

        public static void ToCsv(this IEnumerable list, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(list);
            }
        }

        public static List<object> FromCsv(this string filePath)
        {
            List<object> list = null;

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                list = csv.GetRecords(typeof(object)).ToList();
            }

            return list;
        }

        public static string ToHex(this string asciiString, int fromIndex)
        {
            return asciiString.Substring(Math.Max(0, asciiString.Length - fromIndex)).ToHex();
        }

        public static string ToHex(this string asciiString)
        {
            var builder = new StringBuilder();
            foreach (char c in asciiString)
            {
                int tmp = c;
                builder.Append(string.Format("{0:x2}", System.Convert.ToUInt32(tmp.ToString())));
            }
            return builder.ToString();
        }

        public static double NextDouble(this Random random, double minValue, double maxValue, int decimalCount = 3)
        {
            return Math.Round(random.NextDouble() * (maxValue - minValue) + minValue, decimalCount);
        }

        public static T AddOrUpdate<T>(this ICollection<T> list, T item, Func<T, bool> predicate = null)
        {
            T existing = list.FirstOrDefault(predicate);

            if (existing != null)
            {
                item.CopyPropertiesTo(existing);
            }
            else
            {
                list.Add(item);
            }

            return existing;
        }

        public static void AddOrUpdate<T, W>(this IDictionary<T, W> dictionary, T key, W value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static void RemoveIfExists<T>(this ICollection<T> list, Func<T, bool> predicate)
        {
            T existing = list.FirstOrDefault(predicate);

            if (existing != null)
            {
                list.Remove(existing);
            }
        }

        public static bool AddIfNotExists<T>(this ICollection<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }

            return false;
        }

        public static bool InsertIfNotExists<T>(this IList<T> list, T item, int index)
        {
            if (!list.Contains(item))
            {
                list.Insert(index, item);
                return true;
            }

            return false;
        }

        public static void CopyPropertiesTo(this object source, object destination)
        {
            // If either source or destination is null, throw an exception
            if (source == null || destination == null)
            {
                return;
            }

            // Get the types of the source and destination objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            // Iterate through the properties of the source object
            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue; // Skip properties that cannot be read
                }

                // Find the corresponding property in the destination object
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue; // Skip if the property does not exist in the destination
                }

                if (!targetProperty.CanWrite)
                {
                    continue; // Skip properties that cannot be written to
                }

                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue; // Skip if the set method is private
                }

                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue; // Skip if the property is static
                }

                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue; // Skip if the property types are not compatible
                }

                // Get the value from the source property
                object? sourceValue = srcProp.GetValue(source, null);

                // Handle null values: Optionally, you can log or handle the null differently here
                if (sourceValue == null)
                {
                    continue; // Skip setting if the value is null (optional behavior)
                }

                try
                {
                    // Passed all tests, lets set the value
                    targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
                }
                catch (Exception ex) 
                {
                    Logger.Log(ex);
                    Logger.Log($"propertyName: {srcProp?.Name}\nsource: {source?.ToJson()}\ndestination: {destination?.ToJson()}");
                }
            }
        }

        public static PropertyInfo GetProperty(this object sender, string propertyName)
        {
            return sender?.GetType()?.GetProperty(propertyName);
        }

        public static object GetPropertyValue(this object sender, string propertyName)
        {
            return sender?.GetProperty(propertyName)?.GetValue(sender, null);
        }

        public static void SetPropertyValue(this object sender, string propertyName, object value)
        {
            sender?.GetProperty(propertyName)?.SetValue(sender, value, null);
        }

        public static T GetLast<T>(this ICollection<T> collection)
        {
            return collection?.Count > 0 ? collection.Last() : default;
        }

        public static string ToString(this double? value, int decimalPlaces = 2)
        {
            return value.ToString(decimalPlaces.ToString());
        }

        public static string ToString(this double? value, string decimalPlacesStr = "2")
        {
            return value.WithDecimalPlaces(decimalPlacesStr).ToString();
        }

        public static double? WithDecimalPlaces(this double? value, string decimalPlacesStr = "2")
        {
            if (int.TryParse(decimalPlacesStr, out int decimalPlaces) && value is not null)
            {
                return WithDecimalPlaces(value.Value, decimalPlaces);
            }

            return value;
        }

        public static double WithDecimalPlaces(this double value, int decimalPlaces = 2)
        {
            return (double)Math.Round((decimal)value, decimalPlaces);
        }

        public static bool IsNullOrZero(this double? value)
        {
            return value == null || value == 0;
        }

        public static bool ContainsSubStringOf(this IList<string> list, string text)
        {
            return list?.FirstOrDefault(text.Contains) != null;
        }

        public static string ToSmartString(this double value)
        {
            return value == 0 ? "" : Math.Abs(value).ToString();
        }

        public static string GetSign(this double value)
        {
            return value == 0 ? "" : value > 0 ? "+" : "-";
        }

        public static bool ContainsPlusOrMinus(this string value)
        {
            return value.Contains('+') || value.Contains('-');
        }

        public static string GetContentBeforeSpace(this string content)
        {
            return content.Split(' ').FirstOrDefault();
        }

        public static DateTime ToEasternTime(this DateTime utcTime)
        {
            System.TimeZoneInfo easternZone = System.TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return System.TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternZone);
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }
    }
}