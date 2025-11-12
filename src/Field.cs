using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efferent.HL7.V2
{
    public class Field : MessageElement
    {
        private List<Field> _RepetitionList;

        internal ComponentCollection ComponentList { get; set; }

        public bool IsComponentized { get; set; } = false;
        public bool HasRepetitions { get; set; } = false;
        public bool IsDelimitersField { get; set; } = false;

        internal List<Field> RepetitionList
        {
            get
            {
                if (_RepetitionList == null)
                    _RepetitionList = [];
                    
                return _RepetitionList;
            }
            set
            {
                _RepetitionList = value;
            }
        }

        protected override void ProcessValue()
        {
            if (this.IsDelimitersField)  // Special case for the delimiters fields (MSH)
            {
                var subcomponent = new SubComponent(_value, this.Encoding);

                this.ComponentList.Clear();
                Component component = new Component(this.Encoding, true);

                component.SubComponentList.Add(subcomponent);

                this.ComponentList.Add(component);
                return;
            }

            this.HasRepetitions = _value.Contains(this.Encoding.RepeatDelimiter.ToString());

            if (this.HasRepetitions)
            {
                var individualFields = _value.Split(this.Encoding.RepeatDelimiter);
                _RepetitionList = new List<Field>(individualFields.Length);
                
                for (int index = 0; index < individualFields.Length; index++)
                {
                    Field field = new Field(individualFields[index], this.Encoding);
                    _RepetitionList.Add(field);
                }
            }
            else
            {
                var allComponents = _value.Split(this.Encoding.ComponentDelimiter);

                this.ComponentList = new ComponentCollection(allComponents.Length);

                foreach (string strComponent in allComponents)
                {
                    Component component = new Component(this.Encoding);
                    component.Value = strComponent;
                    this.ComponentList.Add(component);
                }

                this.IsComponentized = this.ComponentList.Count > 1;
            }
        }

        public Field(HL7Encoding encoding)
        {
            this.ComponentList = [];
            this.Encoding = encoding;
        }

        public Field(string value, HL7Encoding encoding)
        {
            this.ComponentList = [];
            this.Encoding = encoding;
            this.Value = value;
        }

        /// <summary>
        /// Adds a new component to the component list of the field.
        /// </summary>
        /// <param name="com">The component to add.</param>
        /// <returns>True if the component was added successfully.</returns>
        /// <exception cref="HL7Exception">Thrown when unable to add the new component.</exception>
        public bool AddNewComponent(Component com)
        {
            try
            {
                this.ComponentList.Add(com);
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new component Error - " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Adds a new component to the component list of the field at the specified position.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <param name="position">The position at which to add the component (1-based index).</param>
        /// <returns>True if the component was added successfully.</returns>
        /// <exception cref="HL7Exception">Thrown when unable to add the new component.</exception>
        public bool AddNewComponent(Component component, int position)
        {
            try
            {
                this.ComponentList.Add(component, position);
                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new component Error - " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves the component at the specified position.
        /// </summary>
        /// <param name="position">The position of the component to retrieve (1-based index).</param>
        /// <returns>The component at the specified position.</returns>
        /// <exception cref="HL7Exception">Thrown when the component is not available at the specified position.</exception>
        public Component Components(int position)
        {
            position--;

            try
            {
                return ComponentList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Component not available Error - " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves all components of the field.
        /// </summary>
        /// <returns>A list of all components in the field.</returns>
        public List<Component> Components()
        {
            return ComponentList;
        }

        /// <summary>
        /// Retrieves all repetitions of the field if any exist.
        /// </summary>
        /// <returns>A list of field repetitions if they exist; otherwise, null.</returns>
        public List<Field> Repetitions()
        {
            if (this.HasRepetitions)
                return RepetitionList;

            return null;
        }

        /// <summary>
        /// Retrieves a specific repetition of the field.
        /// </summary>
        /// <param name="repetitionNumber">The repetition number to retrieve (1-based index).</param>
        /// <returns>The field repetition at the specified repetition number if it exists; otherwise, null.</returns>
        public Field Repetitions(int repetitionNumber)
        {
            if (this.HasRepetitions)
                return RepetitionList[repetitionNumber - 1];

            return null;
        }

        /// <summary>
        /// Removes any trailing components that have empty values from the component list.
        /// </summary>
        /// <returns>True if the operation was successful.</returns>
        /// <exception cref="HL7Exception">Thrown when an error occurs while removing trailing components.</exception>
        public bool RemoveEmptyTrailingComponents()
        {
            try
            {
                for (var eachComponent = ComponentList.Count - 1; eachComponent >= 0; eachComponent--)
                {
                    if (ComponentList[eachComponent].Value == "")
                        ComponentList.Remove(ComponentList[eachComponent]);
                    else
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Error removing trailing components - " + ex.Message, ex);
            }
        }
        
        /// <summary>
        /// Adds a field to the list of repeating fields.
        /// </summary>
        /// <param name="field">The field to add as a repetition.</param>
        /// <exception cref="HL7Exception">
        /// Thrown when the field is not marked as having repetitions (HasRepetitions must be true).
        /// </exception>
        public void AddRepeatingField(Field field) 
        {
            if (!this.HasRepetitions) 
                throw new HL7Exception("Repeating field must have repetitions (HasRepetitions = true)");

            if (_RepetitionList == null) 
                _RepetitionList = []; 

            _RepetitionList.Add(field);
        }

        /// <summary>
        /// Serializes a field into a string with proper encoding.
        /// </summary>
        /// <param name="strMessage">A StringBuilder to write the serialized field into.</param>
        public void SerializeField(StringBuilder strMessage)
        {
            using (var writer = new StringWriter(strMessage))
            {
                this.SerializeFieldAsync(writer).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Asynchronously serializes a field into a TextWriter with proper encoding.
        /// </summary>
        /// <param name="writer">A TextWriter to write the serialized field into.</param>
        /// <returns>A task representing the asynchronous serialization operation.</returns>
        public async Task SerializeFieldAsync(TextWriter writer)
        {
            if (this.ComponentList.Count > 0)
            {
                int indexCom = 0;

                foreach (Component com in this.ComponentList)
                {
                    indexCom++;

                    if (com.SubComponentList.Count > 0)
                        await writer.WriteAsync(string.Join(Encoding.SubComponentDelimiter.ToString(), com.SubComponentList.Select(sc => Encoding.Encode(sc.Value))));
                    else
                        await writer.WriteAsync(Encoding.Encode(com.Value));

                    if (indexCom < this.ComponentList.Count)
                        await writer.WriteAsync(Encoding.ComponentDelimiter);
                }
            }
            else
            {
                await writer.WriteAsync(Encoding.Encode(this.Value));
            }
        }
    }
}
