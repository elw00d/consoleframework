using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConsoleFramework.Core;
using ConsoleFramework.Xaml;

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
    [TypeConverter(typeof(GridLengthTypeConverter))]
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

    public class GridLengthTypeConverter : ITypeConverter
    {
        public bool CanConvertFrom( Type sourceType ) {
            switch (Type.GetTypeCode(sourceType))
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
            }
            return false;
        }

        public bool CanConvertTo( Type destinationType ) {
            return destinationType == typeof ( string );
        }

        public object ConvertFrom( object value ) {
            if ( value is string ) {
                string s = ( string ) value;
                if ( s == "Auto" ) {
                    return new GridLength( GridUnitType.Auto, 0 );
                } else if ( s.EndsWith( "*" ) ) {
                    if ( s == "*" ) 
                        return new GridLength( GridUnitType.Star, 1 );

                    int num = Int32.Parse( s.Substring( 0, s.Length - 1 ) );
                    return new GridLength( GridUnitType.Star, num );
                } else {
                    return new GridLength( GridUnitType.Pixel, Int32.Parse( s ) );
                }
            } else {
                int num = Convert.ToInt32( value );
                return new GridLength( GridUnitType.Pixel, num );
            }
        }

        public object ConvertTo( object value, Type destinationType ) {
            if ( destinationType == typeof ( string ) ) {
                GridLength gl = ( GridLength ) value;
                switch (gl.GridUnitType)
                {
                    case GridUnitType.Auto:
                        return "Auto";

                    case GridUnitType.Star:
                        if (gl.Value == 1) {
                            return "*";
                        }
                        return (Convert.ToString(gl.Value) + "*");
                }
                return Convert.ToString(gl.Value);
            }
            throw new NotSupportedException();
        }
    }

    [ContentProperty("Controls")]
    public class Grid : Control
    {
        private readonly List< ColumnDefinition > columnDefinitions = new List< ColumnDefinition >();
        private readonly List< RowDefinition > rowDefinitions = new List< RowDefinition >();
        private readonly ObservableCollection< Control > children = new ObservableCollection< Control >();

        public List< ColumnDefinition > ColumnDefinitions {
            get { return columnDefinitions; }
        }

        public List< RowDefinition > RowDefinitions {
            get { return rowDefinitions; }
        }

        public IList<Control> Controls { get { return children; } }

        public Grid( ) {
            children.CollectionChanged += ( sender, args ) => {
                if ( null != args.OldItems ) {
                    foreach ( var oldItem in args.OldItems ) {
                        RemoveChild( ( Control ) oldItem );
                    }
                }
                if ( null != args.NewItems ) {
                    foreach ( var newItem in args.NewItems ) {
                        AddChild( ( Control ) newItem );
                    }
                }
            };
        }

        protected override Size MeasureOverride( Size availableSize ) {
            return base.MeasureOverride( availableSize );
        }

        protected override Size ArrangeOverride( Size finalSize ) {
            return base.ArrangeOverride( finalSize );
        }
    }
}
