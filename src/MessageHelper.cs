using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Efferent.HL7.V2
{
    /// <summary>
    /// Provides utility methods for parsing, splitting, formatting, and serializing HL7 messages.
    /// </summary>
    public static class MessageHelper
    {
        private static readonly string[] lineSeparators = { "\r\n", "\n\r", "\r", "\n" };

        /// <summary>
        /// Splits a string into a list of substrings based on the specified string delimiter.
        /// </summary>
        /// <param name="strStringToSplit">The string to split.</param>
        /// <param name="splitBy">The string delimiter to split by.</param>
        /// <param name="splitOptions">Options that specify whether to omit empty substrings from the result.</param>
        /// <returns>A list of substrings obtained by splitting the input string.</returns>
        public static List<string> SplitString(string strStringToSplit, string splitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(new string[] { splitBy }, splitOptions).ToList();
        }

        /// <summary>
        /// Splits a string into a list of substrings based on the specified character delimiter.
        /// </summary>
        /// <param name="strStringToSplit">The string to split.</param>
        /// <param name="chSplitBy">The character delimiter to split by.</param>
        /// <param name="splitOptions">Options that specify whether to omit empty substrings from the result.</param>
        /// <returns>A list of substrings obtained by splitting the input string.</returns>
        public static List<string> SplitString(string strStringToSplit, char chSplitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(new char[] { chSplitBy }, splitOptions).ToList();
        }

        /// <summary>
        /// Splits a string into a list of substrings based on any of the specified character delimiters.
        /// </summary>
        /// <param name="strStringToSplit">The string to split.</param>
        /// <param name="chSplitBy">An array of character delimiters to split by.</param>
        /// <param name="splitOptions">Options that specify whether to omit empty substrings from the result.</param>
        /// <returns>A list of substrings obtained by splitting the input string.</returns>
        public static List<string> SplitString(string strStringToSplit, char[] chSplitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(chSplitBy, splitOptions).ToList();
        }

        /// <summary>
        /// Splits an HL7 message string into its component segments.
        /// </summary>
        /// <param name="message">The HL7 message string to split.</param>
        /// <returns>A list of segment strings extracted from the message, excluding empty or whitespace-only segments.</returns>
        public static List<string> SplitMessage(string message)
        {
            return message.Split(lineSeparators, StringSplitOptions.None).Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
        }

        /// <summary>
        /// Formats a <see cref="DateTime"/> value as an HL7 long date string including fractional seconds.
        /// </summary>
        /// <param name="dt">The <see cref="DateTime"/> value to format.</param>
        /// <returns>A string representation of the date and time in the "yyyyMMddHHmmss.FFFF" HL7 format.</returns>
        public static string LongDateWithFractionOfSecond(DateTime dt)
        {
            return dt.ToString("yyyyMMddHHmmss.FFFF", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Extracts HL7 messages framed with MLLP delimiters from a string containing one or more concatenated messages.
        /// </summary>
        /// <param name="messages">The string containing one or more MLLP-framed HL7 messages.</param>
        /// <returns>An array of HL7 message strings extracted from within the MLLP framing characters.</returns>
        public static string[] ExtractMessages(string messages)
        {
            var expr = "\x0B(.*?)\x1C\x0D";
            var matches = Regex.Matches(messages, expr, RegexOptions.Singleline);
            var list = new List<string>();

            foreach (Match m in matches)
            {
                list.Add(m.Groups[1].Value);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Parses an HL7 date time string into a <see cref="DateTime"/> object, with optional timezone offset handling.
        /// </summary>
        /// <param name="dateTimeString">The date time string to parse.</param>
        /// <param name="throwException">If <c>true</c>, will throw an exception on failure; otherwise, will return <c>null</c>.</param>
        /// <param name="applyOffset"><c>true</c> (default) to apply the timezone offset and return UTC; <c>false</c> to ignore it.</param>
        /// <returns>
        /// A <see cref="DateTime"/> object if parsing succeeds, or <c>null</c> if parsing fails and <paramref name="throwException"/> is <c>false</c>.
        /// When <paramref name="applyOffset"/> is <c>true</c>, returns <see cref="DateTimeKind.Utc"/>.
        /// When <paramref name="applyOffset"/> is <c>false</c>, returns <see cref="DateTimeKind.Unspecified"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When applying the timezone offset, the parsed DateTime is treated at UTC to begin with, and then the offset is subtracted.
        /// </para>
        /// <para>
        /// <strong>Timezone Handling:</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>When <paramref name="applyOffset"/> is <c>true</c>: The method treats the parsed DateTime as being in the specified timezone, then converts it to UTC by subtracting the offset.</description></item>
        /// <item><description>When <paramref name="applyOffset"/> is <c>false</c>: The timezone offset is ignored, and the DateTime is returned as <see cref="DateTimeKind.Unspecified"/>.</description></item>
        /// <item><description>If no timezone is specified in the input string, a zero offset is assumed.</description></item>
        /// </list>
        /// <para>
        /// <strong>Important:</strong> When <paramref name="applyOffset"/> is <c>false</c>, the returned <see cref="DateTimeKind.Unspecified"/> DateTime 
        /// will be treated as local time by .NET methods. This can cause inconsistent behavior across different system timezones. 
        /// It is recommended to use <paramref name="applyOffset"/> = <c>true</c> unless you have specific requirements for preserving the original timezone context.
        /// </para>
        /// </remarks>
        /// <inheritdoc cref="ParseDateTime(string, out TimeZone, bool, bool)" path="/exception[@cref='FormatException']"/>
        public static DateTime? ParseDateTime(string dateTimeString, bool throwException = false, bool applyOffset = true)
        {
            return ParseDateTime(dateTimeString, out TimeSpan offset, throwException, applyOffset);
        }

        /// <summary>
        /// Parses an HL7 date time string into a <see cref="DateTime"/> object and extracts the timezone offset.
        /// </summary>
        /// <param name="dateTimeString">The HL7 date time string to parse (format: YYYY[MM[DD[HH[MM[SS[.FFFF]]]]]][+/-ZZZZ]).</param>
        /// <param name="offset">The timezone offset extracted from the date time string.</param>
        /// <param name="throwException">If <c>true</c>, throws an exception on parse failure; if <c>false</c>, returns <c>null</c>.</param>
        /// <param name="applyOffset"><c>true</c> to apply the timezone offset and return UTC; <c>false</c> (default) to ignore it.</param>
        /// <returns>
        /// A <see cref="DateTime"/> object if parsing succeeds, or <c>null</c> if parsing fails and <paramref name="throwException"/> is <c>false</c>.
        /// When <paramref name="applyOffset"/> is <c>true</c>, returns <see cref="DateTimeKind.Utc"/>.
        /// When <paramref name="applyOffset"/> is <c>false</c>, returns <see cref="DateTimeKind.Unspecified"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When applying the timezone offset, the parsed DateTime is treated at UTC to begin with, and then the offset is subtracted.
        /// </para>
        /// <para>
        /// <strong>Timezone Handling:</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>When <paramref name="applyOffset"/> is <c>true</c>: The method treats the parsed DateTime as being in the specified timezone, then converts it to UTC by subtracting the offset.</description></item>
        /// <item><description>When <paramref name="applyOffset"/> is <c>false</c>: The timezone offset is ignored, and the DateTime is returned as <see cref="DateTimeKind.Unspecified"/>.</description></item>
        /// <item><description>If no timezone is specified in the input string, a zero offset is assumed.</description></item>
        /// </list>
        /// <para>
        /// <strong>Important:</strong> When <paramref name="applyOffset"/> is <c>false</c>, the returned <see cref="DateTimeKind.Unspecified"/> DateTime 
        /// will be treated as local time by .NET methods. This can cause inconsistent behavior across different system timezones. 
        /// It is recommended to use <paramref name="applyOffset"/> = <c>true</c> unless you have specific requirements for preserving the original timezone context.
        /// </para>
        /// </remarks>
        /// <exception cref="FormatException">Thrown when <paramref name="throwException"/> is <c>true</c> and the input string is not in a valid HL7 date time format.</exception>
        public static DateTime? ParseDateTime(string dateTimeString, out TimeSpan offset, bool throwException = false, bool applyOffset = false)
        {
            var expr = @"^\s*((?:18|19|20)[0-9]{2})(?:(1[0-2]|0[1-9])(?:(3[0-1]|[1-2][0-9]|0[1-9])(?:([0-1][0-9]|2[0-3])(?:([0-5][0-9])(?:([0-5][0-9](?:\.[0-9]{1,4})?)?)?)?)?)?)?(?:([+-][0-1][0-9]|[+-]2[0-3])([0-5][0-9]))?\s*$";
            var matches = Regex.Matches(dateTimeString, expr, RegexOptions.Singleline);

            offset = new TimeSpan();

            try
            {
                if (matches.Count != 1)
                    throw new FormatException("Invalid date format");

                var groups = matches[0].Groups;
                int year = int.Parse(groups[1].Value, CultureInfo.InvariantCulture);
                int month = groups[2].Success ? int.Parse(groups[2].Value, CultureInfo.InvariantCulture) : 1;
                int day = groups[3].Success ? int.Parse(groups[3].Value, CultureInfo.InvariantCulture) : 1;
                int hours = groups[4].Success ? int.Parse(groups[4].Value, CultureInfo.InvariantCulture) : 0;
                int mins = groups[5].Success ? int.Parse(groups[5].Value, CultureInfo.InvariantCulture) : 0;

                double secs = groups[6].Success ? double.Parse(groups[6].Value, CultureInfo.InvariantCulture) : 0;
               
                int tzh = groups[7].Success ? int.Parse(groups[7].Value, CultureInfo.InvariantCulture) : 0;
                int tzm = groups[8].Success ? int.Parse(groups[8].Value, CultureInfo.InvariantCulture) : 0;
                offset = new TimeSpan(tzh, tzm, 0);

                if (applyOffset)
                {
                    // When applying offset, convert to UTC
                    return new DateTime(year, month, day, hours, mins, 0, DateTimeKind.Utc).AddSeconds(secs).Subtract(offset);
                }
                else
                {
                    // When not applying offset, return as Unspecified and leave to the caller to handle
                    return new DateTime(year, month, day, hours, mins, 0).AddSeconds(secs);
                }
            }
            catch
            {
                if (throwException)
                    throw;

                return null;
            }
        }

        /// <summary>
        /// Serializes an HL7 message string into an MLLP (Minimal Lower Layer Protocol) framed byte array.
        /// </summary>
        /// <param name="message">The HL7 message string to serialize.</param>
        /// <param name="encoding">The text encoding to use for the message string. Defaults to UTF-8 if <c>null</c>.</param>
        /// <returns>A byte array containing the MLLP-framed HL7 message, including start block, end block, and carriage return delimiters.</returns>
        public static byte[] GetMLLP(string message, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            byte[] data = encoding.GetBytes(message);
            byte[] buffer = new byte[data.Length + 3];
            buffer[0] = 11; // VT

            Array.Copy(data, 0, buffer, 1, data.Length);
            buffer[buffer.Length - 2] = 28; // FS
            buffer[buffer.Length - 1] = 13; // CR

            return buffer;
        }
    }
}
