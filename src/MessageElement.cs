namespace Efferent.HL7.V2
{
    /// <summary>
    /// Serves as the abstract base class for all HL7 message elements,
    /// handling value encoding and decoding.
    /// </summary>
    public abstract class MessageElement
    {
#pragma warning disable CA1051
        protected string _value = string.Empty;
#pragma warning restore CA1051

        /// <summary>
        /// Gets or sets the value of the message element.
        /// Automatically decodes encoded HL7 values when getting,
        /// and triggers <see cref="ProcessValue"/> when setting.
        /// </summary>
        public  string Value
        {
            get
            {
                return _value == Encoding.PresentButNull ? null : Encoding.Decode(_value);
            }
            set
            {
                _value = value;
                ProcessValue();
            }
        }

        /// <summary>
        /// Gets the raw encoded string value without decoding.
        /// </summary>
        public  string UndecodedValue
        {
            get
            {
                return _value == Encoding.PresentButNull ? null : _value;
            }
        }

        /// <summary>
        /// Gets the HL7 encoding characters used for this element.
        /// </summary>
        public HL7Encoding Encoding { get; protected set; }

        /// <summary>
        /// Processes the assigned value.
        /// This abstract method is intended to be implemented by subclasses.
        /// </summary>
        protected abstract void ProcessValue();
    }
}
