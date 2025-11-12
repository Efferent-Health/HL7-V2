using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Efferent.HL7.V2
{
    /// <summary>
    /// Manages HL7 encoding and decoding rules, including field, component, repetition, escape, and subcomponent delimiters.
    /// Provides methods to encode and decode HL7 message content according to these delimiters.
    /// </summary>
    public class HL7Encoding
    {
        /// <summary>
        /// Gets or sets the field delimiter character used to separate fields within a segment.
        /// </summary>
        public char FieldDelimiter { get; set; } = '|';        // \F\
        /// <summary>
        /// Gets or sets the component delimiter character used to separate components within a field.
        /// </summary>
        public char ComponentDelimiter { get; set; } = '^';    // \S\
        /// <summary>
        /// Gets or sets the repeat delimiter character used to separate repeated fields or components.
        /// </summary>
        public char RepeatDelimiter { get; set; } = '~';       // \R\
        /// <summary>
        /// Gets or sets the escape character used to denote escaped sequences within HL7 content.
        /// </summary>
        public char EscapeCharacter { get; set; } = '\\';      // \E\
        /// <summary>
        /// Gets or sets the subcomponent delimiter character used to separate subcomponents within a component.
        /// </summary>
        public char SubComponentDelimiter { get; set; } = '&'; // \T\
        /// <summary>
        /// Gets or sets the segment delimiter string used to separate segments within an HL7 message.
        /// </summary>
        public string SegmentDelimiter { get; set; } = "\r";   // {cr}
        /// <summary>
        /// Gets or sets the string representation for a present but null value in HL7 messages.
        /// </summary>
        public string PresentButNull { get; set; } = "\"\"";   // ""
        /// <summary>
        /// Gets a string containing all delimiter characters used in the current encoding.
        /// </summary>
        public string AllDelimiters => "" + FieldDelimiter + ComponentDelimiter + RepeatDelimiter + (EscapeCharacter == (char)0 ? "" : EscapeCharacter.ToString()) + SubComponentDelimiter;

        private static readonly string[] _segmentDelimiters = { "\r\n", "\n\r", "\r", "\n" };
        private char[] _charsThatMightNeedEncoding;

        public HL7Encoding()
        {
            // set the defaults
            _charsThatMightNeedEncoding = new[] { '<', '\r', '\n', FieldDelimiter, ComponentDelimiter, RepeatDelimiter, EscapeCharacter, SubComponentDelimiter };
        }

        /// <summary>
        /// Evaluates and sets the delimiter characters based on the provided delimiter string.
        /// </summary>
        /// <param name="delimiters">A string containing delimiter characters in the order: field, component, repetition, escape, subcomponent.</param>
        public void EvaluateDelimiters(string delimiters)
        {
            this.FieldDelimiter = delimiters[0];
            this.ComponentDelimiter = delimiters[1];
            this.RepeatDelimiter = delimiters[2];

            if (delimiters[4] == this.FieldDelimiter)
            {
                this.EscapeCharacter = (char)0;
                this.SubComponentDelimiter = delimiters[3];
            }
            else
            {
                this.EscapeCharacter = delimiters[3];
                this.SubComponentDelimiter = delimiters[4];
            }
            _charsThatMightNeedEncoding = new[] { '<', '\r', '\n', FieldDelimiter, ComponentDelimiter, RepeatDelimiter, EscapeCharacter, SubComponentDelimiter };
        }

        /// <summary>
        /// Determines and sets the segment delimiter used in the given HL7 message.
        /// </summary>
        /// <param name="message">The HL7 message string to analyze.</param>
        /// <exception cref="HL7Exception">Thrown when no valid segment delimiter is found in the message.</exception>
        public void EvaluateSegmentDelimiter(string message)
        {
            foreach (var delim in _segmentDelimiters)
            {
                if (message.Contains(delim))
                {
                    SegmentDelimiter = delim;
                    return;
                }
            }

            throw new HL7Exception("Segment delimiter not found in message", HL7Exception.BadMessage);
        }

        /// <summary>
        /// Encodes a string value by escaping HL7 special characters according to the current encoding rules.
        /// </summary>
        /// <param name="val">The string value to encode.</param>
        /// <returns>The encoded string with special characters escaped.</returns>
        public  string Encode(string val)
        {
            if (val == null)
                return PresentButNull;

            if (val.Length == 0)
                return val;

            // If there's nothing that needs encoding, just return the value as-is
            // Disabled as it was breaking the CustomDelimiterTest() test method
            // if (val.IndexOfAny(_charsThatMightNeedEncoding) < 0)
            //     return val;

            var sb = new StringBuilder();

            for (int i = 0; i < val.Length; i++)
            {
                char c = val[i];

                bool continueEncoding = true;
                if (c == '<')
                {
                    continueEncoding = false;
                    // special case <B>
                    if (val.Length >= i + 3 && val[i+1] == 'B' && val[i+2] == '>')
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append('H');
                        sb.Append(this.EscapeCharacter);
                        i += 2; // +1 in loop
                    }
                    // special case </B>
                    else if (val.Length >= i + 4 && val[i + 1] == '/' && val[i + 2] == 'B' && val[i + 3] == '>')
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append('N');
                        sb.Append(this.EscapeCharacter);
                        i += 3; // +1 in loop
                    }
                    // special case <BR>
                    else if (val.Length >= i + 4 && val[i + 1] == 'B' && val[i + 2] == 'R' && val[i + 3] == '>')
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append(".br");
                        sb.Append(this.EscapeCharacter);
                        i += 3; // +1 in loop
                    }
                    else
                    {
                        continueEncoding = true;
                    }
                }

                if (continueEncoding)
                {
                    if (c == this.ComponentDelimiter)
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append('S');
                        sb.Append(this.EscapeCharacter);
                    }
                    else if (c == this.EscapeCharacter)
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append('E');
                        sb.Append(this.EscapeCharacter);
                    }
                    else if (c == this.FieldDelimiter)
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append('F');
                        sb.Append(this.EscapeCharacter);
                    }
                    else if (c == this.RepeatDelimiter)
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append('R');
                        sb.Append(this.EscapeCharacter);
                    }
                    else if (c == this.SubComponentDelimiter)
                    {
                        sb.Append(this.EscapeCharacter);
                        sb.Append('T');
                        sb.Append(this.EscapeCharacter);
                    }
                    else if (c == 10 || c == 13) // All other non-visible characters will be preserved
                    {
                        string v = string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)c);

                        if ((v.Length % 2) != 0) // make number of digits even, this test would only be needed for values > 0xFF
                            v = "0" + v;

                        sb.Append(this.EscapeCharacter);
                        sb.Append('X');
                        sb.Append(v);
                        sb.Append(this.EscapeCharacter);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decodes an encoded HL7 string by converting escaped sequences back to their original characters.
        /// </summary>
        /// <param name="encodedValue">The encoded HL7 string to decode.</param>
        /// <returns>The decoded string with escape sequences replaced by their corresponding characters.</returns>
        public string Decode(string encodedValue)
        {
            if (string.IsNullOrWhiteSpace(encodedValue))
                return encodedValue;

            if (!encodedValue.Contains(EscapeCharacter))
            {
                // no need to decode, just return as is
                return encodedValue;
            }

            var result = new StringBuilder();

            for (int i = 0; i < encodedValue.Length; i++)
            {
                char c = encodedValue[i];

                if (c != this.EscapeCharacter)
                {
                    result.Append(c);
                    continue;
                }

                i++;
                int li = encodedValue.IndexOf(this.EscapeCharacter, i);

                if (li == -1)
                {
                    // throw new HL7Exception("Invalid escape sequence in HL7 string");
                    result.Append(this.EscapeCharacter);

                    if (i<encodedValue.Length)
                        result.Append(encodedValue[i]);
                    continue;
                }

                string seq = encodedValue.Substring(i, li-i);
                i = li;

                if (seq.Length == 0)
                    continue;

                switch (seq)
                {
                    case "H": // Start higlighting
                        result.Append("<B>");
                        break;
                    case "N": // normal text (end highlighting)
                        result.Append("</B>");
                        break;
                    case "F": // field separator
                        result.Append(this.FieldDelimiter);
                        break;
                    case "S": // component separator
                        result.Append(this.ComponentDelimiter);
                        break;
                    case "T": // subcomponent separator
                        result.Append(this.SubComponentDelimiter);
                        break;
                    case "R": // repetition separator
                        result.Append(this.RepeatDelimiter);
                        break;
                    case "E": // escape character
                        result.Append(this.EscapeCharacter);
                        break;
                    case ".br":
                        result.Append("<BR>");
                        break;
                    default:
                        if (seq[0]=='X')
                            result.Append(DecodeHexString(seq.Substring(1)));
                        else
                            result.Append(seq);
                        break;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Decodes a hexadecimal string representation into its corresponding Unicode string.
        /// </summary>
        /// <param name="hex">A string containing hexadecimal digits representing encoded bytes.</param>
        /// <returns>The decoded Unicode string.</returns>
        public static string DecodeHexString(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];

            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            string str;

            if (bytes.Length == 1)
                // str = Encoding.ASCII.GetString(bytes);
                str = char.ConvertFromUtf32(bytes[0]);
            else if (bytes.Length == 2 && bytes[0] == 0)
                str = char.ConvertFromUtf32(bytes[1]);
            else
                str = Encoding.UTF8.GetString(bytes);

            return str;
        }
    }
}
