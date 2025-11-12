using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Efferent.HL7.V2
{
    public class Segment : MessageElement
    {
        internal FieldCollection FieldList { get; set; }
        internal int SequenceNo { get; set; }

        /// <summary>
        /// Gets or sets the name of the HL7 segment.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Segment"/> class with the specified encoding.
        /// </summary>
        /// <param name="encoding">The HL7 encoding rules to use for this segment.</param>
        public Segment(HL7Encoding encoding)
        {
            this.FieldList = new FieldCollection();
            this.Encoding = encoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Segment"/> class with the specified name and encoding.
        /// </summary>
        /// <param name="name">The name of the segment (e.g., "MSH", "PID").</param>
        /// <param name="encoding">The HL7 encoding rules to use for this segment.</param>
        public Segment(string name, HL7Encoding encoding)
        {
            this.FieldList = new FieldCollection();
            this.Name = name;
            this.Encoding = encoding;
        }

        protected override void ProcessValue()
        {
            var allFields = _value.Split(this.Encoding.FieldDelimiter);

            for (int i = 1; i < allFields.Length; i++)
            {
                string strField = allFields[i];
                Field field = new Field(this.Encoding);

                if (Name == "MSH" && i == 1)
                    field.IsDelimitersField = true; // special case

                field.Value = strField;
                this.FieldList.Add(field);
            }

            if (this.Name == "MSH")
            {
                var field1 = new Field(this.Encoding);
                field1.IsDelimitersField = true;
                field1.Value = this.Encoding.FieldDelimiter.ToString();

                this.FieldList.Insert(0, field1);
            }
        }

        /// <summary>
        /// Creates a deep copy of the current segment, including all its fields and encoding.
        /// </summary>
        /// <returns>A new <see cref="Segment"/> instance with the same content and encoding.</returns>
        public Segment DeepCopy()
        {
            var newSegment = new Segment(this.Name, this.Encoding);
            newSegment.Value = this.Value;

            return newSegment;
        }

        /// <summary>
        /// Adds a new empty field to the segment.
        /// </summary>
        public void AddEmptyField()
        {
            this.AddNewField(string.Empty);
        }

        /// <summary>
        /// Adds a new field with the specified content at the end or at the specified position.
        /// </summary>
        /// <param name="content">The string content of the new field.</param>
        /// <param name="position">The one-based position to insert the field at; if -1, adds at the end.</param>
        public void AddNewField(string content, int position = -1)
        {
            this.AddNewField(new Field(content, this.Encoding), position);
        }

        /// <summary>
        /// Adds a new field with the specified content and delimiter flag.
        /// </summary>
        /// <param name="content">The string content of the new field.</param>
        /// <param name="isDelimiters">If true, marks the field as a delimiters field to prevent decoding.</param>
        public void AddNewField(string content, bool isDelimiters)
        {
            var newField = new Field(this.Encoding);

            if (isDelimiters)
                newField.IsDelimitersField = true; // Prevent decoding

            newField.Value = content;
            this.AddNewField(newField, -1);
        }

        /// <summary>
        /// Adds a new <see cref="Field"/> to the segment at the specified position or at the end.
        /// </summary>
        /// <param name="field">The <see cref="Field"/> object to add.</param>
        /// <param name="position">The one-based position to insert the field at; if -1, adds at the end.</param>
        /// <returns>True if the field was added successfully.</returns>
        /// <exception cref="HL7Exception">Thrown if the field cannot be added to the segment.</exception>
        public bool AddNewField(Field field, int position = -1)
        {
            try
            {
                if (position < 0)
                {
                    this.FieldList.Add(field);
                }
                else
                {
                    position--;
                    this.FieldList.Add(field, position);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Unable to add new field in segment " + this.Name + " Error - " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves the field at the specified one-based position within the segment.
        /// </summary>
        /// <param name="position">The one-based position of the field to retrieve.</param>
        /// <returns>The <see cref="Field"/> at the specified position.</returns>
        /// <exception cref="HL7Exception">Thrown if the field at the specified position is not available.</exception>
        public Field Fields(int position)
        {
            position--;

            try
            {
                return this.FieldList[position];
            }
            catch (Exception ex)
            {
                throw new HL7Exception("Field not available Error - " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets all fields contained within the segment.
        /// </summary>
        /// <returns>A list of all <see cref="Field"/> objects in the segment.</returns>
        public List<Field> GetAllFields()
        {
            return this.FieldList;
        }

        /// <summary>
        /// Gets the sequence number of the segment within the message.
        /// </summary>
        /// <returns>The sequence number as an integer.</returns>
        public int GetSequenceNo()
        {
            return this.SequenceNo;
        }

        /// <summary>
        /// Serializes the segment into a string builder with proper HL7 encoding.
        /// </summary>
        /// <param name="strMessage">The <see cref="StringBuilder"/> to write the serialized segment to.</param>
        public void SerializeSegment(StringBuilder strMessage)
        {
            using (var writer = new StringWriter(strMessage))
            {
                this.SerializeSegmentAsync(writer).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Asynchronously serializes the segment into a <see cref="TextWriter"/> with proper HL7 encoding.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the serialized segment to.</param>
        /// <returns>A task representing the asynchronous serialization operation.</returns>
        public async Task SerializeSegmentAsync(TextWriter writer)
        {        
            await writer.WriteAsync(this.Name);

            if (this.FieldList.Count > 0)
                await writer.WriteAsync(Encoding.FieldDelimiter);

            int startField = this.Name == "MSH" ? 1 : 0;

            for (int i = startField; i < this.FieldList.Count; i++)
            {
                if (i > startField)
                    await writer.WriteAsync(Encoding.FieldDelimiter);

                var field = this.FieldList[i];

                if (field.IsDelimitersField)
                {
                    await writer.WriteAsync(field.UndecodedValue);
                    continue;
                }

                if (field.HasRepetitions)
                {
                    for (int j = 0; j < field.RepetitionList.Count; j++)
                    {
                        if (j > 0)
                            await writer.WriteAsync(Encoding.RepeatDelimiter);

                        await field.RepetitionList[j].SerializeFieldAsync(writer);
                    }
                }
                else
                {
                    await field.SerializeFieldAsync(writer);
                }
            }

            await writer.WriteAsync(Encoding.SegmentDelimiter);
        }
    }
}
