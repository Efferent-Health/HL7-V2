using System.Collections.Generic;

namespace Efferent.HL7.V2
{
    /// <summary>
    /// Represents a collection of HL7 fields with custom indexing and insertion logic.
    /// </summary>
    internal sealed class FieldCollection : List<Field>
    {
        /// <summary>
        /// Gets or sets the field at the specified index. Returns null if the index is out of range.
        /// </summary>
        internal new Field this[int index]
        {
            get
            {
                Field field = null;

                if (index < base.Count)
                    field = base[index];

                return field;
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Adds a field at the next available position.
        /// </summary>
        /// <param name="field">The field to add.</param>
        internal new void Add(Field field)
        {
            base.Add(field);
        }

        /// <summary>
        /// Adds a field at the specified position. If the position is beyond the current list size,
        /// the list is expanded with blank fields to accommodate the new field.
        /// </summary>
        /// <param name="field">The field to add.</param>
        /// <param name="position">The zero-based position at which to insert the field.</param>
        internal void Add(Field field, int position)
        {
            int listCount = base.Count;

            if (position < listCount)
            {
                base[position] = field;
            }
            else
            {
                for (int fieldIndex = listCount; fieldIndex < position; fieldIndex++)
                {
                    Field blankField = new Field(string.Empty, field.Encoding);
                    base.Add(blankField);
                }

                base.Add(field);
            }
        }
    }
}
