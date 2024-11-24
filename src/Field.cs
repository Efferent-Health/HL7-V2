using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public List<Component> Components()
        {
            return ComponentList;
        }

        public List<Field> Repetitions()
        {
            if (this.HasRepetitions)
                return RepetitionList;

            return null;
        }

        public Field Repetitions(int repetitionNumber)
        {
            if (this.HasRepetitions)
                return RepetitionList[repetitionNumber - 1];

            return null;
        }

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
        
        public void AddRepeatingField(Field field) 
        {
            if (!this.HasRepetitions) 
                throw new HL7Exception("Repeating field must have repetitions (HasRepetitions = true)");

            if (_RepetitionList == null) 
                _RepetitionList = []; 

            _RepetitionList.Add(field);
        }

        /// <summary>
        /// Serializes a field into a string with proper encoding
        /// </summary>
        /// <param name="strMessage">A StringBuilder to write on</param>
        public void SerializeField(StringBuilder strMessage)
        {
            if (this.ComponentList.Count > 0)
            {
                int indexCom = 0;

                foreach (Component com in this.ComponentList)
                {
                    indexCom++;

                    if (com.SubComponentList.Count > 0)
                        strMessage.Append(string.Join(Encoding.SubComponentDelimiter.ToString(), com.SubComponentList.Select(sc => Encoding.Encode(sc.Value))));
                    else
                        strMessage.Append(Encoding.Encode(com.Value));

                    if (indexCom < this.ComponentList.Count)
                        strMessage.Append(Encoding.ComponentDelimiter);
                }
            }
            else
            {
                strMessage.Append(Encoding.Encode(this.Value));
            }
        }
    }
}
