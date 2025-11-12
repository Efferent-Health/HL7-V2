namespace Efferent.HL7.V2
{
    /// <summary>
    /// Represents the smallest data unit in an HL7 message, specifically a subcomponent within a component.
    /// </summary>
    public class SubComponent : MessageElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubComponent"/> class with the specified value and encoding.
        /// </summary>
        /// <param name="val">The string value of the subcomponent.</param>
        /// <param name="encoding">The HL7 encoding rules to apply.</param>
        public SubComponent(string val, HL7Encoding encoding)
        {
            this.Encoding = encoding;
            this.Value = val;
        }

        /// <summary>
        /// Processes or transforms the subcomponent's value when it is assigned.
        /// This method is overridden to implement any specific processing logic for the subcomponent.
        /// </summary>
        protected override void ProcessValue()
        {
        }
    }
}
