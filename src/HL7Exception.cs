using System;

namespace Efferent.HL7.V2
{
    public class HL7Exception : Exception
    {
        public const string RequiredFieldMissing = "Validation Error - Required field missing in message";
        public const string UnsupportedMessageType = "Validation Error - Message Type not supported by this implementation";
        public const string BadMessage = "Validation Error - Bad Message";
        public const string ParsingError = "Parsing Error";
        public const string SerializationError = "Serialization Error";

        public string ErrorCode { get; set; }

        public HL7Exception(string message, Exception inner = null) : base(message, inner)
        {
        }

        public HL7Exception(string message, string Code, Exception inner = null) : base(message, inner)
        {
            ErrorCode = Code;
        }

        public override string ToString()
        {
            return ErrorCode + " : " + Message;
        }
    }
}
