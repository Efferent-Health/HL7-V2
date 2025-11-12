using System;
using System.Collections.Generic;

namespace Efferent.HL7.V2
{
    /// <summary>
    /// Represents a Component of an HL7 V2 field. A Component may contain
    /// one or more SubComponents separated by the encoding's subcomponent delimiter.
    /// </summary>
    public class Component : MessageElement
    {
        /// <summary>
        /// Internal list of subcomponents for this component.
        /// </summary>
        internal List<SubComponent> SubComponentList { get; set; }

        /// <summary>
        /// True when this component contains multiple subcomponents.
        /// </summary>
        public bool IsSubComponentized { get; set; } = false;

        private bool isDelimiter = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Component"/> class.
        /// Use this constructor when constructing a delimiter marker or when the
        /// value will be set/processed later.
        /// </summary>
        /// <param name="encoding">The HL7 encoding instance describing delimiters.</param>
        /// <param name="isDelimiter">If true, this instance represents a delimiter token rather than a parsed value.</param>
        public Component(HL7Encoding encoding, bool isDelimiter = false)
        {
            this.isDelimiter = isDelimiter;
            this.SubComponentList = new List<SubComponent>();
            this.Encoding = encoding;
        }

        /// <summary>
        /// Initializes a new <see cref="Component"/> from a raw string value and encoding.
        /// The value will be processed into subcomponents when required.
        /// </summary>
        /// <param name="pValue">The raw component string value.</param>
        /// <param name="encoding">The HL7 encoding instance describing delimiters.</param>
        public Component(string pValue, HL7Encoding encoding)
        {
            this.SubComponentList = new List<SubComponent>();
            this.Encoding = encoding;
            this.Value = pValue;
        }

        /// <summary>
        /// Parse the component's current value into SubComponent objects using the
        /// encoding's SubComponentDelimiter. If the instance was created as a delimiter
        /// token, the whole value is treated as a single subcomponent.
        /// </summary>
        protected override void ProcessValue()
        {
            string[] allSubComponents;

            if (this.isDelimiter)
                allSubComponents = new string[] { this.Value }; // fixed single-element array initializer
            else
                allSubComponents = _value.Split(this.Encoding.SubComponentDelimiter);

            if (allSubComponents.Length > 1)
                this.IsSubComponentized = true;

            SubComponentList.Clear(); // in case there's existing data in there
            SubComponentList.Capacity = allSubComponents.Length;

            foreach (string strSubComponent in allSubComponents)
            {
                SubComponent subComponent = new SubComponent(strSubComponent, this.Encoding);
                SubComponentList.Add(subComponent);
            }
        }

        /// <summary>
        /// Returns the SubComponent at the given 1-based position.
        /// Throws an <see cref="HL7Exception"/> if the requested position is not available.
        /// </summary>
        /// <param name="position">1-based index of the subcomponent to retrieve.</param>
        /// <returns>The <see cref="SubComponent"/> at the requested position.</returns>
        public SubComponent SubComponents(int position)
        {
            position--;

            try
            {
                return SubComponentList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("SubComponent not available Error-" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns the full list of SubComponents for this Component.
        /// </summary>
        /// <returns>List of <see cref="SubComponent"/> objects (may be empty).</returns>
        public List<SubComponent> SubComponents()
        {
            return SubComponentList;
        }
    }
}