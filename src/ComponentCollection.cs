using System.Collections.Generic;

namespace Efferent.HL7.V2
{
    /// <summary>
    /// Represents a collection of HL7 components with safe indexing and positional insertion.
    /// </summary>
    internal sealed class ComponentCollection : List<Component>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentCollection"/> class.
        /// </summary>
        public ComponentCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentCollection"/> class with the specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial number of components the collection can contain.</param>
        public ComponentCollection(int initialCapacity) : base(initialCapacity)
        {
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// Returns null if the index is out of range.
        /// </summary>
        /// <param name="index">The zero-based index of the component to get or set.</param>
        /// <returns>The component at the specified index, or null if index is out of range.</returns>
        internal new Component this[int index]
        {
            get
            {
                Component component = null;

                if (index < base.Count)
                    component = base[index];

                return component;
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Adds a component to the end of the collection.
        /// </summary>
        /// <param name="component">The component to add.</param>
        internal new void Add(Component component)
        {
            base.Add(component);
        }

        /// <summary>
        /// Adds a component at the specified position in the collection.
        /// If the position is beyond the current count, intermediate positions are filled with blank components.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <param name="position">The one-based position at which to insert the component.</param>
        internal void Add(Component component, int position)
        {
            int listCount = base.Count;
            position = position - 1;

            if (position < listCount)
            {
                base[position] = component;
            }
            else
            {
                for (int comIndex = listCount; comIndex < position; comIndex++)
                {
                    Component blankComponent = new Component(component.Encoding);
                    blankComponent.Value = string.Empty;
                    base.Add(blankComponent);
                }

                base.Add(component);
            }
        }
    }
}
