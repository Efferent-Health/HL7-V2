using System;

namespace Efferent.HL7.V2
{
    /// <summary>
    /// Represents errors that occur during HL7 message processing, including parsing, validation, or serialization errors.
    /// </summary>
    public class HL7Exception : Exception
    {
        /// <summary>
        /// Indicates a validation error due to a required field missing in the message.
        /// </summary>
        public const string RequiredFieldMissing = "Validation Error - Required field missing in message";

        /// <summary>
        /// Indicates a validation error where the message type is not supported by this implementation.
        /// </summary>
        public const string UnsupportedMessageType = "Validation Error - Message Type not supported by this implementation";

        /// <summary>
        /// Indicates a validation error due to a malformed or invalid message.
        /// </summary>
        public const string BadMessage = "Validation Error - Bad Message";

        /// <summary>
        /// Indicates an error occurred during parsing of the HL7 message.
        /// </summary>
        public const string ParsingError = "Parsing Error";

        /// <summary>
        /// Indicates an error occurred during serialization of the HL7 message.
        /// </summary>
        public const string SerializationError = "Serialization Error";

        /// <summary>
        /// Gets or sets the error code categorizing the HL7 error.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HL7Exception"/> class with a specified error message and an optional inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or null if no inner exception is specified.</param>
        public HL7Exception(string message, Exception inner = null) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HL7Exception"/> class with a specified error message, error code, and an optional inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="Code">The error code categorizing the HL7 error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or null if no inner exception is specified.</param>
        public HL7Exception(string message, string Code, Exception inner = null) : base(message, inner)
        {
            ErrorCode = Code;
        }

        /// <summary>
        /// Returns a string that represents the current exception, including the error code and message.
        /// </summary>
        /// <returns>A formatted string containing the error code and the error message.</returns>
        public override string ToString()
        {
            return ErrorCode + " : " + Message;
        }
    }
}
