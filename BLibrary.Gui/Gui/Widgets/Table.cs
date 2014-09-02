/*
* Copyright (c) 2014 SirSengir
* Starliners (http://github.com/SirSengir/Starliners)
*
* This file is part of Starliners.
*
* Starliners is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* Starliners is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Starliners.  If not, see <http://www.gnu.org/licenses/>.
*/

﻿using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using BLibrary.Graphics;
using BLibrary.Util;
using BLibrary.Gui.Backgrounds;

namespace BLibrary.Gui.Widgets {

    public delegate void TableEventHandler (Table sender, EventArgs args);

    public sealed class Table : Scrollable {
        #region Classes & Enums

        public interface IPopulator {
            void Populate (Table table);
        }

        enum ColumnSizing : byte {
            None,
            Absolute,
            Relative,
            Auto
        }

        ///<summary>
        /// Defines a single column.
        /// </summary>
        sealed class ColumnDefinition {
            public uint Ordinal { get; set; }

            public ColumnSizing Sizing { get; set; }

            public int Width { get; set; }

            public Background Background { get; set; }

            public Vect2i Padding { get; set; }

            public ColumnDefinition (uint ordinal) {
                Ordinal = ordinal;
                Sizing = ColumnSizing.Auto;
                Padding = new Vect2i (8, 0);
            }

            public ColumnDefinition (uint ordinal, int fixedWidth) {
                Ordinal = ordinal;
                Width = fixedWidth;
                Sizing = ColumnSizing.Absolute;
            }

            public int CalculateWidth (int availableWidth) {
                if (Sizing == ColumnSizing.Absolute)
                    return Width;

                return (int)Math.Round (((float)availableWidth / 100) * Width); // Rounding errors are significant otherwise!
            }
        }

        sealed class Row {
            public readonly Dictionary<uint, Widget> Columns = new Dictionary<uint, Widget> ();
            public EventArgs HoverArgs;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current row count in this table.
        /// </summary>
        /// <value>The row count.</value>
        public int RowCount {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the height of rows in this table.
        /// </summary>
        /// <value>The height of the row.</value>
        public uint RowHeight {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the outer margin of rows.
        /// </summary>
        /// <value>The margin.</value>
        public uint Margin {
            get;
            set;
        }

        protected override Rect2i ViewportArea {
            get {
                return base.ViewportArea + new Rect2i (Margin, Margin, -2 * Margin, -2 * Margin);
            }
        }

        /// <summary>
        /// Gets or sets the background to draw behind unhovered, unselected rows.
        /// </summary>
        /// <value>The row dormant.</value>
        public Background RowDormant { get; set; }

        /// <summary>
        /// Gets or sets the background to draw behind hovered, unselected rows.
        /// </summary>
        /// <value>The row dormant.</value>
        public Background RowHighlight { get; set; }

        /// <summary>
        /// Gets or sets the background used to draw behind the currently selected row.
        /// </summary>
        /// <value>The row marking.</value>
        public Background RowMarking { get; set; }

        /// <summary>
        /// Gets or sets the background used to draw behind alternating rows.
        /// </summary>
        /// <value>The row alternating.</value>
        public Background RowAlternating { get; set; }

        /// <summary>
        /// Gets a value indicating whether a row in this table is currently selected.
        /// </summary>
        /// <value><c>true</c> if this instance has selection; otherwise, <c>false</c>.</value>
        public bool HasSelection {
            get { return _selectedRow >= 0; }
        }

        #endregion

        #region Fields

        Dictionary<int, Row> _table = new Dictionary<int, Row> ();
        int _selectedRow = -1;
        int _hoveredRow = -1;
        uint _column = 0;
        Dictionary<uint, ColumnDefinition> _columns = new Dictionary<uint, ColumnDefinition> ();
        bool _columnWidthsDirty = true;
        Dictionary<uint, int> _calculatedColumnWidths = new Dictionary<uint, int> ();
        Dictionary<int, Background> _backgrounds = new Dictionary<int, Background> ();
        int _columnCount = 0;

        #endregion

        #region Constructor

        public Table (Vect2i position, Vect2i size)
            : base (position, size, string.Empty) {
            Margin = 4;
            _columns [_column] = new ColumnDefinition (_column);
            RefreshDimensions ();
        }

        #endregion

        /// <summary>
        /// Resets the table.
        /// </summary>
        public void Reset () {
            ClearWidgets ();
            _table.Clear ();
            //_selectedRow = -1;
            //_hoveredRow = -1;
            _column = 0;
            _columnWidthsDirty = true;
            _calculatedColumnWidths.Clear ();
            _columnCount = 0;
            _backgrounds.Clear ();
            RowCount = 0;
            IsGenerated = false;
            RefreshDimensions ();
        }

        public void Reset (IPopulator populator) {
            Reset ();
            populator.Populate (this);
        }

        #region Events

        /// <summary>
        /// Fired when the gui element is (newly) hovered over.
        /// </summary>
        public event TableEventHandler RowHovered;

        void OnRowHovered (int row) {
            if (RowHovered != null && _table.ContainsKey (row)) {
                RowHovered (this, _table [row].HoverArgs);
            }
        }

        protected override void OnResized () {
            base.OnResized ();
            _columnWidthsDirty = true;
        }

        #endregion

        #region Control

        public void SetRowHoverArgs (EventArgs args) {
            if (!_table.ContainsKey (RowCount)) {
                _table [RowCount] = new Row ();
            }

            _table [RowCount].HoverArgs = args;
        }

        /// <summary>
        /// Set a column to the specified fixed with.
        /// </summary>
        /// <param name="ordinal">Ordinal.</param>
        /// <param name="width">Width.</param>
        public void SetColumnAbsolute (uint ordinal, int width) {
            if (!_columns.ContainsKey (ordinal)) {
                _columns [ordinal] = new ColumnDefinition (ordinal);
            }

            _columns [ordinal].Sizing = ColumnSizing.Absolute;
            _columns [ordinal].Width = width;
            _columnWidthsDirty = true;
        }

        public void SetColumnRelative (uint ordinal, int percent) {
            if (!_columns.ContainsKey (ordinal))
                _columns [ordinal] = new ColumnDefinition (ordinal);

            _columns [ordinal].Sizing = ColumnSizing.Relative;
            _columns [ordinal].Width = percent;
            _columnWidthsDirty = true;
        }

        public void SetColumnPadding (uint ordinal, Vect2i padding) {
            if (!_columns.ContainsKey (ordinal))
                _columns [ordinal] = new ColumnDefinition (ordinal);

            _columns [ordinal].Padding = padding;
            _columnWidthsDirty = true;
        }

        public void SetColumnBackground (uint ordinal, Background background) {
            if (!_columns.ContainsKey (ordinal))
                _columns [ordinal] = new ColumnDefinition (ordinal);

            _columns [ordinal].Background = background;
        }

        public void SetRowBackground (Background background) {
            _backgrounds [RowCount] = background;
        }

        public void NextRow () {
            RowCount++;
            _column = 0;
            RefreshDimensions ();
        }

        public void NextColumn () {
            _column++;
            if (!_columns.ContainsKey (_column)) {
                _columns [_column] = new ColumnDefinition (_column);
            }
        }

        /// <summary>
        /// Sets the first row as selected, without clicking it.
        /// </summary>
        public void SelectFirst () {
            _selectedRow = 0;
        }

        public void SelectLast () {
            _selectedRow = RowCount;
        }

        public void AddCellContent (Widget widget) {
            //base.AddWidget (widget);
            if (!_table.ContainsKey (RowCount)) {
                _table [RowCount] = new Row ();
            }
            _table [RowCount].Columns [_column] = widget;

            _table.Keys.ToList ().Sort ();
            _table [RowCount].Columns.Keys.ToList ().Sort ();
        }

        public void AddIntertitle (string intertitle) {
            AddCellContent (new ListItemText (Vect2i.ZERO, new Vect2i (EffectiveSize.X - 2 * Margin, RowHeight), "intertitle", intertitle) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            }
            );
            SetRowBackground (UIProvider.Backgrounds ["guiButtonHovered"].Copy ());
            NextRow ();
        }

        #endregion

        protected override void Regenerate () {
            base.Regenerate ();
            foreach (var row in _table) {
                foreach (var column in row.Value.Columns) {
                    AddWidget (column.Value);
                }
            }

            VerifyWidths ();
            SetCellSizes ();
        }

        void VerifyWidths () {
            if (_columnWidthsDirty) {
                RecalculateColumnWidths ();
                _columnCount = 0;
                foreach (Row row in _table.Values) {
                    int cs = (int)row.Columns.Max (p => p.Key) + 1;
                    _columnCount = cs > _columnCount ? cs : _columnCount;
                }
                _columnWidthsDirty = false;
            }
        }

        void SetCellSizes () {
            foreach (KeyValuePair<int, Row> row in _table) {
                foreach (KeyValuePair<uint, Widget> entry in row.Value.Columns) {

                    Widget widget = entry.Value;
                    Vect2i padding = _columns [entry.Key].Padding;

                    // Get the with of the cell
                    int cellwidth = _calculatedColumnWidths [entry.Key] - 2 * padding.X;
                    for (uint i = entry.Key + 1; i < _columnCount; i++) {
                        if (!row.Value.Columns.ContainsKey (i)) {
                            cellwidth += _calculatedColumnWidths [i];
                        } else {
                            break;
                        }
                    }
                    Vect2i cellSize = new Vect2i (cellwidth, (widget.Size.Y > RowHeight ? (uint)widget.Size.Y : RowHeight) - 2 * padding.Y);
                    widget.Size = cellSize;
                }
            }

        }

        protected override void DrawPort (RenderTarget target, RenderStates states) {

            VerifyWidths ();

            uint yAxis = 0;
            RenderStates rstates = states;
            rstates.Transform.Translate (Margin, Margin);

            foreach (KeyValuePair<int, Row> row in _table) {
                uint rowHeight = 0;
                int cellStep = 0;

                if (IsVisibleRow (yAxis, RowHeight)) {
                    Background bg = null;
                    if (_backgrounds.ContainsKey (row.Key)) {
                        bg = _backgrounds [row.Key];
                    } else if (_selectedRow == row.Key && RowMarking != null) {
                        bg = RowMarking;
                    } else if (RowAlternating != null && row.Key % 2 == 0) {
                        bg = RowAlternating;
                    } else if (_hoveredRow == row.Key && RowHighlight != null) {
                        bg = RowHighlight;
                    } else if (RowDormant != null) {
                        bg = RowDormant;
                    }

                    if (bg != null) {
                        bg.Render (new Vect2i (0, yAxis), new Vect2i (EffectiveSize.X - 2 * Margin, RowHeight), target, rstates, bg.Colour);
                    }
                }

                foreach (KeyValuePair<uint, Widget> entry in row.Value.Columns) {

                    Widget widget = entry.Value;
                    Vect2i padding = _columns [entry.Key].Padding;
                    Vect2i cellPosition = new Vect2i (cellStep + padding.X, yAxis + padding.Y);

                    // Adjust the widget to render.
                    widget.ValidateElement ();
                    widget.PositionRelative = cellPosition;

                    // Note the consumed space.
                    cellStep += _calculatedColumnWidths [entry.Key];
                    rowHeight = widget.Size.Y > rowHeight ? (uint)widget.Size.Y : RowHeight;

                    if (IsVisibleRow (yAxis, rowHeight)) {
                        // Draw background if needed.
                        if (_columns [entry.Key].Background != null) {
                            _columns [entry.Key].Background.Render (cellPosition, widget.Size, target, rstates, this);
                        }
                        // Draw the widget.
                        widget.Draw (target, rstates);
                    }
                }

                yAxis += rowHeight + Margin;
            }
        }

        bool IsVisibleRow (uint yAxis, uint rowheight) {
            return yAxis + rowheight > Scroll.Y && yAxis < Scroll.Y + Size.Y;
        }

        /// <summary>
        /// Determines which table row the given element is located in, -1 if none.
        /// </summary>
        /// <returns>The row.</returns>
        /// <param name="element">Element.</param>
        public int DetermineRow (GuiElement element) {
            for (int count = 0; count < _table.Count; count++) {
                foreach (Widget widget in _table[count].Columns.Values) {
                    if (widget == element) {
                        return count;
                    }
                }
            }

            return -1;
        }

        protected override Vect2f DetermineDimensions (int fixedWidth) {
            return new Vect2f (fixedWidth, RowCount * (RowHeight + Margin));
        }

        void RecalculateColumnWidths () {
            _calculatedColumnWidths.Clear ();

            int areSet = 0;

            int remainingSpace = (int)(EffectiveSize.X - 2 * Margin);
            int usedSpace = 0;

            // Set fixed widths first.
            for (uint i = 0; i < _columns.Count; i++) {
                if (_columns [i].Sizing == ColumnSizing.Absolute) {
                    _calculatedColumnWidths [i] = _columns [i].CalculateWidth (remainingSpace);
                    usedSpace += _calculatedColumnWidths [i];
                    areSet++;
                }
            }

            remainingSpace -= usedSpace;
            usedSpace = 0;

            // Set percentage based widths.
            for (uint i = 0; i < _columns.Count; i++) {
                if (_columns [i].Sizing == ColumnSizing.Relative) {
                    _calculatedColumnWidths [i] = _columns [i].CalculateWidth (remainingSpace);
                    usedSpace += _calculatedColumnWidths [i];
                    areSet++;
                }
            }

            remainingSpace -= usedSpace;
            usedSpace = 0;

            // Partition the remaining space to other columns.
            for (uint i = 0; i < _columns.Count; i++) {
                if (_columns [i].Sizing == ColumnSizing.Auto) {
                    _calculatedColumnWidths [i] = (int)((float)remainingSpace / (_columns.Count - areSet));
                }
            }
        }

        public override bool HandleMouseMove (Vect2i coordinates) {
            VerifyInteraction (coordinates, false, false);

            if (!IntersectsWith (coordinates)) {
                _hoveredRow = -1;
                return false;
            }

            // First let children determine whether they are hovered.
            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                Children [i].HandleMouseMove (coordinates);
            }

            // Now search for a row with a hovered child.
            int hoveredRow = -1;
            uint currentY = 0;
            for (int count = 0; count < _table.Count; count++) {
                currentY += RowHeight + Margin;
                if (PositionAbsolute.Y + PositionShift.Y + currentY > coordinates.Y) {
                    hoveredRow = count;
                    break;
                }
            }

            if (hoveredRow != _hoveredRow) {
                OnRowHovered (hoveredRow);
            }
            _hoveredRow = hoveredRow;

            return false;
        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            VerifyInteraction (coordinates, false, false);

            if (!IntersectsWith (coordinates)) {
                return false;
            }

            // First let children determine whether they handle that click.
            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                Children [i].HandleMouseClick (coordinates, button);
            }

            _selectedRow = -1;
            uint currentY = 0;
            for (int count = 0; count < _table.Count; count++) {
                currentY += RowHeight + Margin;
                if (PositionAbsolute.Y + PositionShift.Y + currentY > coordinates.Y) {
                    _selectedRow = count;
                    break;
                }
            }
            return true;
        }
    }
}
