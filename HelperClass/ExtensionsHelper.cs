
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;



namespace CustomTypeExtensions
{

    //Extension methods must be defined in a static class
    public class Validator
    {

        public List<string> errors;
        public dynamic Check(dynamic dynValue, bool required = true)
        {
            string dynValueStr = dynValue;
            if (string.IsNullOrEmpty(dynValueStr) && required)
            {
                if (errors == null)
                    errors = new List<string>();
                errors.Add("Property -" + dynValue.Path + "- is required!");

            }
            return dynValue;
            //row new Exception("[Error:FieldRequired]");
        }


        public dynamic Check(dynamic dynValue, string propertyName = null, bool required = true)
        {
            string dynValueStr = dynValue;
            if (string.IsNullOrEmpty(dynValueStr) && required)
            {
                if (errors == null)
                    errors = new List<string>();
                if (!string.IsNullOrEmpty(propertyName))
                    errors.Add("Property -" + propertyName + "- is required!");
                else
                    errors.Add("Property -" + dynValue.Path + "- is required!");
            }
            return dynValue;
            //row new Exception("[Error:FieldRequired]");
        }

    }
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }
    }

    public static class Transform
    {

        public static DateTime? ReformatEZP_DT(dynamic dynDateStr, string DestFormat = "yyyy-MM-ddTHH:mm:ss.fff")
        {
            string lDateStr = dynDateStr;
            DateTime? result = null;
            if (!string.IsNullOrEmpty(lDateStr))
            {
                //25.04.2022 ob 24:00:00 uri
                lDateStr = lDateStr.Replace(" ob ", " ");
                lDateStr = lDateStr.Replace(" uri", "");
                string[] datetimeArr = lDateStr.Split(' ');
                if (datetimeArr.Count() > 1 && datetimeArr[1].StartsWith("24"))
                    result = datetimeArr[0].AsDateTime().Value.AddDays(1);
                else
                    result = lDateStr.AsDateTime();
            }
            return result;
        }


        public static string NullIfEmpty(dynamic str)
        {
            string result = str.Trim();
            if (string.IsNullOrEmpty(result))
            {
                result = null;
            }
            return result;
        }

    }

    public static class StringExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.


        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            return items.Contains(item);
        }






        public static decimal? AsDecimal(this string inputStr, decimal? default_ = null)
        {
            decimal? result = default_;
            if (!string.IsNullOrEmpty(inputStr))
                result = decimal.Parse(inputStr);
            return result;
        }

        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[] { item };
            }
            T[] result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = item;
            return result;
        }

        
        public static bool AsBool(this object input, bool default_ = false)
        {
            bool result = default_;
            if (input != null && input is bool)
                result = (bool)input;
            else if (input != null && input is string)
                result = bool.Parse(input.ToString());
            return result;
        }

        public static decimal? AsDecimal(this object input, decimal? default_ = null)
        {
            if (input == null)
                return default_;

            switch (input)
            {
                case decimal d:
                    return d;
                case float f: // System.Single
                    return (decimal)f;
                case double db:
                    return (decimal)db;
                case int i:
                    return i;
                case long l:
                    return l;
                case string s when decimal.TryParse(s, out var parsed):
                    return parsed;
                default:
                    return default_;
            }
        }


        public static decimal AsDecimal(this decimal? value, decimal defaultValue = 0m)
        {
            return value ?? defaultValue;
        }

        public static int? ToInt(this object input, int? default_ = null)
        {
            if (input == null || input == DBNull.Value)
                return default_;

            try
            {
                return Convert.ToInt32(input);
            }
            catch
            {
                return default_;
            }
        }


        public static DateTime? ToDateTime(this object input)
        {
            DateTime? result = null;
            if (input != null && input is DateTime)
                result = (DateTime)input;
            return result;
        }


        public static decimal Round2(this decimal value)
        {
            return Math.Round(value, 2);
        }

        public static decimal? Round2(this decimal? value)
        {
            decimal? result = value;
            if (result.HasValue)
                result = Math.Round(value.Value, 2);
            return result;
        }

        public static DateTime? AsDateTime(this string date, string DestFormat = "yyyy-MM-ddTHH:mm:ss.fff")
        {
            DateTime? result = null;
            List<string> FormatList = new List<string>();
            FormatList.Add("yyyy/MM/dd");
            FormatList.Add("dd/MM/yyyy");
            FormatList.Add("yyyy-MM-dd");
            FormatList.Add("yyyy-MM-dd HH:mm:ss.fff");
            FormatList.Add("yyyy-MM-dd HH:mm:ss");
            FormatList.Add("yyyy-MM-ddTHH:mm:ss.fff");
            FormatList.Add("yyyy-MM-dd HH:mm:ss");
            FormatList.Add("dd.MM.yyyy");
            FormatList.Add("dd.MM.yyyy HH:mm:ss.fff");
            FormatList.Add("dd.MM.yyyy HH:mm:ss");
            //dtString = dtString.Replace(" 00:00:00", "").Replace(" ","");
            if (!string.IsNullOrEmpty(date))
            {
                date = date.Replace(". ", ".");
                DateTime dt_;
                if (DateTime.TryParseExact(date, FormatList.ToArray(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt_))
                    result = dt_;
            }
            return result;
        }





    }
}



