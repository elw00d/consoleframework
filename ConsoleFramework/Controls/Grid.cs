﻿using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleFramework.Core;
using ConsoleFramework.Rendering;
using Xaml;

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
        public ColumnDefinition( ) {
            Width = new GridLength(GridUnitType.Auto, 0);
        }

        public GridLength Width { get; set; }

        public int? MinWidth { get; set; }

        public int? MaxWidth { get; set; }
    }

    public class RowDefinition
    {
        public RowDefinition( ) {
            Height = new GridLength(GridUnitType.Auto, 0);
        }

        public GridLength Height { get; set; }

        public int? MinHeight { get; set; }

        public int? MaxHeight { get; set; }
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
        private readonly UIElementCollection children;
        private int[ ] columnsWidths;
        private int[ ] rowsHeights;

        public List< ColumnDefinition > ColumnDefinitions {
            get { return columnDefinitions; }
        }

        public List< RowDefinition > RowDefinitions {
            get { return rowDefinitions; }
        }

        public UIElementCollection Controls { get { return children; } }

        public Grid( ) {
            children = new UIElementCollection(this);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>(ColumnDefinitions);
            List<RowDefinition> rows = new List<RowDefinition>(RowDefinitions);
            if (rows.Count == 0)
                rows.Add(new RowDefinition());
            if (columns.Count == 0)
                columns.Add(new ColumnDefinition());
            //if (columns.Count == 0 || rows.Count == 0 )
            //    return Size.Empty;
            Control[,] matrix = new Control[columns.Count, rows.Count];
            for (int x = 0; x < columns.Count; x++)
            {
                for (int y = 0; y < rows.Count; y++)
                {
                    if (Children.Count > y * columns.Count + x)
                    {
                        matrix[x, y] = Children[y * columns.Count + x];
                    }
                    else
                    {
                        matrix[x, y] = null;
                    }
                }
            }
            // Если в качестве availableSize передано PositiveInfinity, мы просто игнорируем Star-элементы,
            // работая с ними так же как с Auto и производим обычное размещение
            bool interpretStarAsAuto = availableSize.Width == int.MaxValue
                                       || availableSize.Height == int.MaxValue;

            // Сначала выполняем Measure всех контролов с учётом ограничений,
            // определённых в columns и rows
            for (int x = 0; x < columns.Count; x++)
            {
                ColumnDefinition columnDefinition = columns[x];

                int width = columnDefinition.Width.GridUnitType == GridUnitType.Pixel
                    ? columnDefinition.Width.Value : int.MaxValue;

                for (int y = 0; y < rows.Count; y++)
                {
                    RowDefinition rowDefinition = rows[y];

                    int height = rowDefinition.Height.GridUnitType == GridUnitType.Pixel
                        ? rowDefinition.Height.Value : int.MaxValue;

                    // Apply min-max constraints
                    if (columnDefinition.MinWidth != null && width < columnDefinition.MinWidth.Value)
                        width = columnDefinition.MinWidth.Value;
                    if (columnDefinition.MaxWidth != null && width > columnDefinition.MaxWidth.Value)
                        width = columnDefinition.MaxWidth.Value;

                    if (rowDefinition.MinHeight != null && height < rowDefinition.MinHeight.Value)
                        height = rowDefinition.MinHeight.Value;
                    if (rowDefinition.MaxHeight != null && height > rowDefinition.MaxHeight.Value)
                        height = rowDefinition.MaxHeight.Value;

                    if (matrix[x, y] != null)
                        matrix[x, y].Measure(new Size(width, height));
                }
            }

            // Теперь для каждого столбца (не-Star) нужно вычислить максимальный Width, а для
            // каждой строки - максимальный Height - эти значения и станут соответственно
            // шириной и высотой ячеек, определяемых координатами строки и столбца

            columnsWidths = new int[columns.Count];

            for (int x = 0; x < columns.Count; x++)
            {
                if (columns[x].Width.GridUnitType != GridUnitType.Star || interpretStarAsAuto)
                {
                    int maxWidth = columns[x].Width.GridUnitType == GridUnitType.Pixel
                                       ? columns[x].Width.Value
                                       : 0;
                    // Учитываем MinWidth. MaxWidth учитывать специально не нужно, поскольку мы это
                    // уже сделали при первом Measure, и DesiredSize не может быть больше MaxWidth
                    if (columns[x].MinWidth != null && maxWidth < columns[x].MinWidth.Value)
                        maxWidth = columns[x].MinWidth.Value;
                    for (int y = 0; y < rows.Count; y++)
                    {
                        if (matrix[x, y] != null)
                            if (matrix[x, y].DesiredSize.Width > maxWidth)
                                maxWidth = matrix[x, y].DesiredSize.Width;
                    }
                    columnsWidths[x] = maxWidth;
                }
            }

            rowsHeights = new int[rows.Count];

            for (int y = 0; y < rows.Count; y++)
            {
                if (rows[y].Height.GridUnitType != GridUnitType.Star || interpretStarAsAuto)
                {
                    int maxHeight = rows[y].Height.GridUnitType == GridUnitType.Pixel
                                        ? rows[y].Height.Value
                                        : 0;
                    if (rows[y].MinHeight != null && maxHeight < rows[y].MinHeight.Value)
                        maxHeight = rows[y].MinHeight.Value;
                    for (int x = 0; x < columns.Count; x++)
                    {
                        if (matrix[x, y] != null)
                            if (matrix[x, y].DesiredSize.Height > maxHeight)
                                maxHeight = matrix[x, y].DesiredSize.Height;
                    }
                    rowsHeights[y] = maxHeight;
                }
            }

            // Теперь вычислим размеры Star-столбцов и Star-строк
            if (!interpretStarAsAuto)
            {
                int totalWidthStars = 0;
                foreach (var columnDefinition in columns)
                {
                    if (columnDefinition.Width.GridUnitType == GridUnitType.Star)
                    {
                        totalWidthStars += columnDefinition.Width.Value;
                    }
                }
                int remainingWidth = Math.Max(0, availableSize.Width - columnsWidths.Sum());
                for (int x = 0; x < columns.Count; x++)
                {
                    ColumnDefinition columnDefinition = columns[x];
                    if (columnDefinition.Width.GridUnitType == GridUnitType.Star)
                    {
                        columnsWidths[x] = remainingWidth * columnDefinition.Width.Value / totalWidthStars;
                    }
                }

                int totalHeightStars = 0;
                foreach (var rowDefinition in rows)
                {
                    if (rowDefinition.Height.GridUnitType == GridUnitType.Star)
                    {
                        totalHeightStars += rowDefinition.Height.Value;
                    }
                }
                int remainingHeight = Math.Max(0, availableSize.Height - rowsHeights.Sum());
                for (int y = 0; y < rows.Count; y++)
                {
                    RowDefinition rowDefinition = rows[y];
                    if (rowDefinition.Height.GridUnitType == GridUnitType.Star)
                    {
                        rowsHeights[y] = remainingHeight * rowDefinition.Height.Value / totalHeightStars;
                    }
                }
            }

            // Окончательный повторный вызов Measure для всех детей с уже определёнными размерами,
            // теми, которые будут использоваться при размещении
            for (int x = 0; x < columns.Count; x++)
            {
                int width = columnsWidths[x];
                for (int y = 0; y < rows.Count; y++)
                {
                    int height = rowsHeights[y];
                    if (matrix[x, y] != null)
                        matrix[x, y].Measure(new Size(width, height));
                }
            }

            return new Size(columnsWidths.Sum(), rowsHeights.Sum());
        }
        protected override Size ArrangeOverride( Size finalSize ) {
            int currentX = 0;
            for ( int x = 0; x < columnsWidths.Length; x++ ) {
                int currentY = 0;
                for ( int y = 0; y < rowsHeights.Length; y++ ) {
                    if ( Children.Count > y*columnsWidths.Length + x ) {
                        Children[y * columnsWidths.Length + x].Arrange( new Rect(
                            new Point(currentX, currentY),
                            new Size(columnsWidths[x], rowsHeights[y])
                            ) );
                    }
                    currentY += rowsHeights[ y ];
                }
                currentX += columnsWidths[ x ];
            }
            return new Size(columnsWidths.Sum(), rowsHeights.Sum());
        }

        public override void Render( RenderingBuffer buffer ) {
            buffer.SetOpacityRect( 0, 0, ActualWidth, ActualHeight, 2 );
        }
    }
}
