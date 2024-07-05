namespace Efferent.HL7.V2
{
    public abstract class MessageElement
    {
#pragma warning disable CA1051
        protected string _value = string.Empty;
#pragma warning restore CA1051

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

        public  string UndecodedValue
        {
            get
            {
                return _value == Encoding.PresentButNull ? null : _value;
            }
        }

        public HL7Encoding Encoding { get; protected set; }

        protected abstract void ProcessValue();
    }
}
