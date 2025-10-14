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
    public class Validator
    {
        public List<string> errors;

        public dynamic Check(dynamic dynValue, bool required = true)
        {
            string dynValueStr = dynValue;
            if (string.IsNullOrEmpty(dynValueStr) && required)
            {
                errors ??= new List<string>();
                errors.Add("Property -" + dynValue.Path + "- is required!");
            }
            return dynValue;
        }

        public dynamic Check(dynamic dynValue, string propertyName = null, bool required = true)
        {
            string dynValueStr = dynValue;
            if (string.IsNullOrEmpty(dynValueStr) && required)
            {
                errors ??= new List<string>();
                if (!string.IsNullOrEmpty(propertyName))
                    errors.Add("Property -" + propertyName + "- is required!");
                else
                    errors.Add("Property -" + dynValue.Path + "- is required!");
            }
            return dynValue;
        }
    }

    public static class DecimalExtensions
    {
        // Za CSS širino
        public static string ToCssPercentValue(this decimal? value)
            => value.HasValue
                ? value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : "0";

        // Za prikaz uporabniku
        public static string ToLocalizedPercent(this decimal? value)
            => value.HasValue
                ? $"{value.Value:0.#} %"
                : "0 %";
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
                lDateStr = lDateStr.Replace(" ob ", " ").Replace(" uri", "");
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
            return string.IsNullOrEmpty(result) ? null : result;
        }
    }

    public static class StringExtension
    {
        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            return items.Contains(item);
        }



        public static decimal? AsDecimal(this string inputStr,
            CultureInfo? culture = null,
            decimal? default_ = null,
            NumberStyles styles =
                NumberStyles.AllowLeadingWhite |
                NumberStyles.AllowTrailingWhite |
                NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint) // brez tisočic
        {
            if (string.IsNullOrWhiteSpace(inputStr)) return default_;

            var usedCulture = culture ?? CultureInfo.InvariantCulture;

            if (decimal.TryParse(inputStr, styles, usedCulture, out var parsed))
                return parsed;

            // če ni podana kultura in niz vsebuje vejico, poskusi sl-SI
            if (culture == null && inputStr.Contains(','))
            {
                var sl = new CultureInfo("sl-SI");
                if (decimal.TryParse(inputStr, styles, sl, out parsed))
                    return parsed;

                // ali normaliziraj na piko in ponovno poskusi z InvariantCulture
                var normalized = inputStr.Replace(',', '.');
                if (decimal.TryParse(normalized, styles, CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }

            return default_;
        }

        public static string AsPercent(this decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "%";
        }

        /// <summary>
        /// Pretvori nullable decimal v niz za CSS width (%), ali vrne prazen string.
        /// </summary>
        public static string AsPercent(this decimal? value)
        {
            return value.HasValue
                ? value.Value.ToString(CultureInfo.InvariantCulture) + "%"
                : string.Empty;

        }





        public static decimal? AsDecimal(this object inputObj, CultureInfo? culture = null, decimal ? default_ = null )
        {
            if (inputObj == null || inputObj == DBNull.Value)
                return default_;

            switch (inputObj)
            {
                case decimal d:
                    return d;

                case int i:
                    return i;

                case long l:
                    return l;

                case float f:
                    return (decimal)f;

                case double db:
                    return (decimal)db;

                case string s:
                    {
                        return s.AsDecimal();
                    }

                default:
                    try
                    {
                        // zadnji poskus: Convert.ToDecimal z InvariantCulture
                        return Convert.ToDecimal(inputObj, culture ?? CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return default_;
                    }
            }
        }


        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null) return new T[] { item };
            T[] result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = item;
            return result;
        }

        public static bool AsBool(this object input, bool default_ = false)
        {
            if (input is bool b) return b;
            if (input is string s && bool.TryParse(s, out var parsed)) return parsed;
            return default_;
        }

        // 🔹 object → decimal? (privzeto InvariantCulture za stringe)


        public static decimal AsDecimal(this decimal? value, decimal defaultValue = 0m)
        {
            return value ?? defaultValue;
        }

        public static int? ToInt(this object input, int? default_ = null)
        {
            if (input == null || input == DBNull.Value) return default_;
            try { return Convert.ToInt32(input); }
            catch { return default_; }
        }

        public static string AsString (this decimal? input, decimal defaultVal=0)
        {
           if (!input.HasValue)
                return defaultVal.ToString();
            return input.ToString();
        }

        public static DateTime? ToDateTime(this object input)
        {
            return input is DateTime dt ? dt : (DateTime?)null;
        }

        public static decimal Round2(this decimal value) => Math.Round(value, 2);

        public static decimal? Round2(this decimal? value) => value.HasValue ? Math.Round(value.Value, 2) : value;

        public static DateTime? AsDateTime(this string date, string DestFormat = "yyyy-MM-ddTHH:mm:ss.fff")
        {
            if (string.IsNullOrEmpty(date)) return null;
            date = date.Replace(". ", ".");
            var formats = new[]
            {
                "yyyy/MM/dd","dd/MM/yyyy","yyyy-MM-dd","yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss","yyyy-MM-ddTHH:mm:ss.fff","dd.MM.yyyy",
                "dd.MM.yyyy HH:mm:ss.fff","dd.MM.yyyy HH:mm:ss"
            };
            return DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt_)
                ? dt_
                : (DateTime?)null;
        }
    }
}
