using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Describes the kind of value that a GridLength object is holding.
    /// </summary>
    public enum GridUnitType
    {
        /// <summary>
        /// The size is determined by the size properties of the content object.
        /// </summary>
        Auto,
        /// <summary>
        /// The value is expressed as a pixel.
        /// </summary>
        Pixel,
        /// <summary>
        /// The value is expressed as a weighted proportion of available space.
        /// </summary>
        Star
    }

    /// <summary>
    /// Represents the length of elements that explicitly support Star unit types.
    /// </summary>
    public struct GridLength
    {
        private readonly GridUnitType gridUnitType;
        private readonly int value;

        public GridLength( GridUnitType unitType, int value ) {
            this.gridUnitType = unitType;
            this.value = value;
        }

        public GridUnitType GridUnitType {
            get { return gridUnitType; }
        }

        public int Value {
            get { return value; }
        }
    }

    public class ColumnDefinition
    {
        public GridLength Width { get; set; }

        public int MinWidth { get; set; }

        public int MaxWidth { get; set; }
    }

    public class RowDefinition
    {
        public GridLength Height { get; set; }

        public int MinHeight { get; set; }

        public int MaxHeight { get; set; }
    }

    public class Grid : Control
    {
        private readonly List< ColumnDefinition > columnDefinitions = new List< ColumnDefinition >();
        private readonly List< RowDefinition > rowDefinitions = new List< RowDefinition >();

        public List< ColumnDefinition > ColumnDefinitions {
            get { return columnDefinitions; }
        }

        public List< RowDefinition > RowDefinitions {
            get { return rowDefinitions; }
        }

        protected override Size MeasureOverride( Size availableSize ) {
            return base.MeasureOverride( availableSize );
        }

        protected override Size ArrangeOverride( Size finalSize ) {
            return base.ArrangeOverride( finalSize );
        }
    }
}
