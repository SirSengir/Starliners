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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BLibrary.Json;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui.Data;
using BLibrary.Util;
using Starliners;
using BLibrary.Gui.Interface;
using Starliners.Util;
using Starliners.States;
using Starliners.Graphics;

namespace BLibrary.Gui {

    public sealed class GuiManager {
        #region Constants

        static readonly Vect2i WINDOW_SHUFFLE = new Vect2i (-32, 32);

        #endregion

        public static GuiManager Instance {
            get;
            set;
        }

        #region Properties

        /// <summary>
        /// Currently active tooltip.
        /// </summary>
        public Tooltip Tooltip {
            get { return _tooltip; }
            set {
                _tooltip = value;
                if (_tooltip != null) {
                    _tooltip.OnShown ();
                }
            }
        }

        /// <summary>
        /// Mouse position during the last frame.
        /// </summary>
        public Vect2i LastMousePos { get; set; }

        /// <summary>
        /// Mouse position this frame.
        /// </summary>
        Vect2i MousePos { get; set; }

        public Vect2i MouseScreenPosition {
            get;
            private set;
        }

        public Vect2i MouseGuiPosition {
            get {
                return MousePos;
            }
        }

        /// <summary>
        /// Counter indicating how long the current mouse button has been kept pressed.
        /// </summary>
        int MouseHeld {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether all gui elements need to be cleared. (Used when changing game state.)
        /// </summary>
        public bool NeedsWipe {
            get;
            set;
        }

        public IInterfaceDefinition UIProvider {
            get;
            private set;
        }

        /// <summary>
        /// ControlState on the Keyboard in the current frame.
        /// </summary>
        public ControlState KeyboardState {
            get;
            private set;
        }

        public Dictionary<int, LegendElement> Legends {
            get {
                return _legends;
            }
        }

        #endregion

        #region Fields

        RenderScene _scene;
        HeldObject _heldObject;
        Tooltip _tooltip;
        bool _displayProfiling;

        string[] _informations = new string[7];
        TextBuffer[] _buffers = new TextBuffer[7];

        const int DRAG_DELAY = 20;
        int _dragDelay;
        IDraggable _draggedElement;

        Dictionary<UITicket, GuiWindow> _windows = new Dictionary<UITicket, GuiWindow> ();
        Dictionary<int, LegendElement> _legends = new Dictionary<int, LegendElement> ();
        GuiWindow[] _sorted;
        GuiWindow _lastActive;

        #endregion

        #region Constructor

        public GuiManager (RenderScene scene, IInterfaceDefinition uiProvider) {
            _scene = scene;
            UIProvider = uiProvider;
            _heldObject = new HeldObject ();

            LastMousePos = (Vect2i)(scene.Size / 2);
            MousePos = LastMousePos;

            LoadGuiPositions ();
        }

        #endregion

        Vect2i AdjustCoords (int screenX, int screenY) {
            return (Vect2i)_scene.MapPixelToCoords (new Vect2i (screenX, screenY)/*, _view*/);
        }

        #region Dragging

        public bool HasDraggedElement { get { return _draggedElement != null; } }

        public bool DelayedDrag {
            get {
                _dragDelay++;
                return !HasDraggedElement && _dragDelay >= DRAG_DELAY;
            }
        }

        void ResetDragDelay () {
            _dragDelay = 0;
        }

        public void SetDraggedElement (IDraggable element) {
            if (element != null) {
                element.BeingDragged = true;
            } else if (_draggedElement != null) {
                _draggedElement.BeingDragged = false;
            }

            _draggedElement = element;
            ResetDragDelay ();
        }

        void UpdateHeldAndDragged (Vect2i mousePos) {
            if (!HasDraggedElement)
                return;

            Vect2i delta = new Vect2i (
                               mousePos.X - LastMousePos.X,
                               mousePos.Y - LastMousePos.Y
                           );
            _draggedElement.Move (delta);
        }

        #endregion

        #region Control State

        void SetKeyboardState () {
            ControlState state = ControlState.None;
            KeyboardState keyboard = Keyboard.GetState ();

            if (keyboard.IsKeyDown (Key.LShift) || keyboard.IsKeyDown (Key.RShift)) {
                state |= ControlState.Shift;
            }
            if (keyboard.IsKeyDown (Key.LControl) || keyboard.IsKeyDown (Key.RControl)) {
                state |= ControlState.Control;
            }
            if (keyboard.IsKeyDown (Key.LAlt) || keyboard.IsKeyDown (Key.RAlt)) {
                state |= ControlState.Alt;
            }

            KeyboardState = state;
        }

        /// <summary>
        /// Combines the current input into a single ControlState for use in interface and simulator.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public ControlState CombineControlState (MouseButton button) {
            ControlState state = KeyboardState;

            if (button == MouseButton.Left) {
                state |= ControlState.MouseLeft;
            }
            if (button == MouseButton.Right) {
                state |= ControlState.MouseRight;
            }
            if (button == MouseButton.Middle) {
                state |= ControlState.Control;
            }

            return state;
        }

        void VerifyTooltip () {
            if (Tooltip != null && !Tooltip.Controller.SustainsTooltip (MousePos)) {
                Tooltip = null;
            }
        }

        #endregion

        #region Interaction

        public void HandleMapMoved () {
            VerifyTooltip ();
        }

        #region Keyboard events

        public bool KeypressBlocked { get; set; }

        public bool HandleTextEntered (char unicode) {
            foreach (GuiWindow element in SortedWindows) {
                if (element.HandleTextEntered (unicode)) {
                    KeypressBlocked = true;
                    return true;
                }
            }

            return false;
        }

        public bool HandleKeyPress (Key key) {

            // Let widgets handle first.
            foreach (GuiWindow element in SortedWindows) {
                if (element.HandleKeyPress (key)) {
                    return true;
                }
            }

            if (key == Key.LShift || key == Key.RShift) {
                return true;
            }

            // Further keypresses are only handled if not blocked.
            if (KeypressBlocked) {
                KeypressBlocked = false;
                return true;
            }

            if (key == Key.Escape && AttemptGuiClose ()) {
                return true;
            } else if (key == Key.B) {
                _displayProfiling = !_displayProfiling;
                return true;
            } else if (GameAccess.Game.HandleKeyboardShortcut (Conversion.KeyToKeysU (key))) {
                return true;
            } else if (key == Key.Escape) {
                // We couldn't close anything, so we want to show the exit menu.
                OpenGui (new GuiExit ());
                return true;
            }

            return false;
        }

        bool AttemptGuiClose () {

            foreach (GuiWindow window in SortedWindows) {
                if (!window.IsCloseable) {
                    continue;
                }
                if (window.IsCompacted) {
                    continue;
                }
                CloseGui (window);
                return true;
            }

            return false;
        }

        #endregion

        #region Mouse events

        void UpdateMousePos (int screenX, int screenY) {
            MouseScreenPosition = new Vect2i (screenX, screenY);
            LastMousePos = MousePos;
            MousePos = AdjustCoords (screenX, screenY);
        }

        /// <summary>
        /// Handles a mouse click.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if mouse click was handled, <c>false</c> otherwise.
        /// </returns>
        /// <param name='screenX'>
        /// Screen x.
        /// </param>
        /// <param name='screenY'>
        /// Screen y.
        /// </param>
        /// <param name='button'>
        /// Button.
        /// </param>
        public bool HandleMouseClick (int screenX, int screenY, MouseButton button) {

            MouseHeld++;
            UpdateMousePos (screenX, screenY);

            if (button == MouseButton.Right && _heldObject.CloseOnRightClick) {
                GameAccess.Interface.Controller.ClearHeld ();
                return true;
            }

            foreach (GuiWindow element in SortedWindows) {
                if (element.HandleMouseClick (this, MousePos, button)) {
                    BringToFront (element);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles release of a mouse button.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if mouse release was handled, <c>false</c> otherwise.
        /// </returns>
        /// <param name='screenX'>
        /// Screen x.
        /// </param>
        /// <param name='screenY'>
        /// Screen y.
        /// </param>
        /// <param name='button'>
        /// Button.
        /// </param>
        public bool HandleMouseRelease (int screenX, int screenY, MouseButton button) {

            MouseHeld = 0;
            UpdateMousePos (screenX, screenY);

            if (HasDraggedElement) {
                SetDraggedElement (null);
                return true;
            }

            foreach (GuiWindow element in SortedWindows) {
                if (element.HandleMouseRelease (this, MousePos, button))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Handles mouse movement.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if mouse move was handled, <c>false</c> otherwise.
        /// </returns>
        /// <param name='screenX'>
        /// Screen x.
        /// </param>
        /// <param name='screenY'>
        /// Screen y.
        /// </param>
        public bool HandleMouseMove (int screenX, int screenY) {

            UpdateMousePos (screenX, screenY);

            // Handle dragged element and exit if so
            UpdateHeldAndDragged (MousePos);
            if (HasDraggedElement) {
                return true;
            } else {
                ResetDragDelay ();
            }

            // Check for tooltip
            bool createdTooltip = false;
            if (Tooltip != null) {
                VerifyTooltip ();
            } else {
                foreach (GuiWindow element in SortedWindows) {
                    Tooltip tooltip = element.CreateTooltip (MousePos);
                    if (tooltip != null) {
                        Tooltip = tooltip;
                        createdTooltip = true;
                    }
                    if (!element.IsDisembodied && element.IntersectsWith (MousePos))
                        break;
                }
            }

            // Handle the mouse movement widget specific
            foreach (GuiWindow element in SortedWindows) {
                if (element.HandleMouseMove (MousePos)) {
                    return true;
                }
            }

            return createdTooltip;
        }

        /// <summary>
        /// Handles mouse wheel movement.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if mouse wheel was handled, <c>false</c> otherwise.
        /// </returns>
        /// <param name='screenX'>
        /// Screen x.
        /// </param>
        /// <param name='screenY'>
        /// Screen y.
        /// </param>
        /// <param name='delta'>
        /// Delta.
        /// </param>
        public bool HandleMouseWheel (int screenX, int screenY, int delta) {
            UpdateMousePos (screenX, screenY);

            foreach (GuiWindow element in SortedWindows) {
                if (element.HandleMouseWheel (this, MousePos, delta)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles the mouse leaving the screen.
        /// </summary>
        /// <returns></returns>
        public bool HandleMouseLeave () {
            Tooltip = null;
            return false;
        }

        #endregion

        #endregion

        #region Element Managment

        GuiWindow[] SortedWindows {
            get {
                return _sorted;
            }
        }

        void SortWindows () {
            _sorted = _windows.OrderBy (p => p.Value.Presets.Group).ThenByDescending (p => p.Value.Ordering).Select (p => p.Value).ToArray ();
            DetermineActive ();
        }

        /// <summary>
        /// Closes all gui elements and resets the GuiManager.
        /// </summary>
        public void Wipe () {
            _legends.Clear ();
            _windows.Clear ();
            Tooltip = null;
            _draggedElement = null;

            // Readd the chat window
            OpenGui (GuiChat.Instance);
        }

        int ResetGuiOrder (ScreenGroup group) {
            IEnumerable<GuiWindow> existing = _windows.Values.Where (p => p.Presets.Group == group).OrderBy (p => p.Ordering);
            // Sequence the existing elements, preserving their current order.
            int count = 0;
            foreach (GuiWindow window in existing) {
                window.Ordering = count;
                count++;
            }
            return count;
        }

        public void BringToFront (GuiWindow window) {
            window.Ordering = ResetGuiOrder (window.Presets.Group);
        }

        void EnsureVisibility (GuiWindow window) {
            while (HasCollisions (window))
                ;
        }

        void DetermineActive () {
            GuiWindow window = _windows.Values.Where (p => p.Presets.Group == ScreenGroup.Windows && !p.IsCompacted).OrderByDescending (p => p.Ordering).FirstOrDefault ();
            // Don't want to trigger the activation action if unneeded.
            if (window == _lastActive) {
                return;
            }
            _lastActive = window;

            if (window != null) {
                window.DoAction (Container.ACTION_ACTIVATE);
            }
        }

        bool HasCollisions (GuiWindow window) {
            foreach (GuiWindow existing in _windows.Values) {
                if (existing == window) {
                    continue;
                }

                Vect2i diff = existing.PositionRelative - window.PositionRelative;
                if (Math.Abs (diff.X) > Math.Abs (WINDOW_SHUFFLE.X / 2) || Math.Abs (diff.Y) > Math.Abs (WINDOW_SHUFFLE.Y / 2)) {
                    continue;
                }

                window.PositionRelative += WINDOW_SHUFFLE;
                return true;
            }

            return false;
        }

        public void OpenMessageBox (ITextProvider title, ITextProvider message, params Widget.ClickAction[] args) {
            OpenGui (new GuiMessageBox (title, message, args));
        }

        /// <summary>
        /// Opens the given gui window.
        /// </summary>
        /// <param name="window">Window.</param>
        public void OpenGui (GuiWindow window) {
            OpenGui (window, -1);
        }

        /// <summary>
        /// Opens the given gui window with the given container id.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="element"></param>
        public void OpenGui (GuiWindow window, int containerId) {
            Tooltip = null;

            // Close the old gui properly, if any.
            CloseGui (containerId);

            if (window == null) {
                throw new SystemException ("Failed to add a null gui as.");
            }

            _windows [new UITicket (window.Presets.Key) { ContainerId = containerId }] = window;
            BringToFront (window);
            EnsureVisibility (window);
            SortWindows ();
        }

        /// <summary>
        /// Closes a gui identified by the given gui key.
        /// </summary>
        /// <param name="windowKey">Window key.</param>
        public void CloseGui (string windowKey) {

            foreach (KeyValuePair<UITicket, GuiWindow> entry in _windows) {
                if (entry.Key.Key != windowKey) {
                    continue;
                }

                CleanupGui (entry.Key);
                break;
            }

        }

        /// <summary>
        /// Closes a gui identified by the given container id.
        /// </summary>
        /// <param name="containerId">Container identifier.</param>
        public void CloseGui (int containerId) {

            foreach (KeyValuePair<UITicket, GuiWindow> entry in _windows) {
                if (entry.Key.ContainerId != containerId) {
                    continue;
                }

                CleanupGui (entry.Key);
                break;
            }

        }

        /// <summary>
        /// Closes a gui identified by the given window.
        /// </summary>
        /// <param name="window">Window.</param>
        public void CloseGui (GuiWindow window) {

            foreach (KeyValuePair<UITicket, GuiWindow> entry in _windows) {
                if (entry.Value != window) {
                    continue;
                }
                CleanupGui (entry.Key);
                break;
            }

        }

        List<UITicket> _ticketRemoval = new List<UITicket> ();

        void CleanupGuis () {
            for (int i = 0; i < _ticketRemoval.Count; i++) {
                CleanupGui (_ticketRemoval [i]);
            }
            _ticketRemoval.Clear ();
        }

        void CleanupGui (UITicket ticket) {
            // Save window position as new default if the window had been moved by the player.
            if (_windows [ticket].WasMoved) {
                SetGuiPosition (_windows [ticket].Presets, _windows [ticket].PositionRelative);
            }
            // Remove it from the listing.
            _windows.Remove (ticket);

            Tooltip = null;

            // Notify the simulator if a container was attached.
            if (ticket.ContainerId >= 0) {
                if (MapState.Instance != null
                    && MapState.Instance.Controller != null)
                    MapState.Instance.Controller.ClosedGui (ticket.ContainerId);
            }

            // Sort windows
            SortWindows ();
        }

        /// <summary>
        /// Closes all Guis
        /// </summary>
        public void CloseGuiAll () {

            foreach (KeyValuePair<UITicket, GuiWindow> entry in _windows) {
                if (!entry.Value.IsCloseable) {
                    continue;
                }
                if (entry.Value.IsCompacted) {
                    continue;
                }
                _ticketRemoval.Add (entry.Key);
            }

            CleanupGuis ();
        }

        #endregion

        #region Positioning

        Dictionary<Vect2i, Dictionary<string, Vect2i>> _positions = new Dictionary<Vect2i, Dictionary<string, Vect2i>> ();

        Vect2i GetUpperRightPosition (Vect2i size) {
            Vect2i windowSize = GameAccess.Interface.WindowSize;
            return new Vect2i ((windowSize.X - size.X - 16), 32);
        }

        Vect2i GetLowerLeftPosition (Vect2i size) {
            Vect2i windowSize = GameAccess.Interface.WindowSize;
            return new Vect2i (16, windowSize.Y - size.Y - 16);
        }

        Vect2i GetLowerRightPosition (Vect2i size) {
            Vect2i windowSize = GameAccess.Interface.WindowSize;
            return new Vect2i (windowSize.X - 16 - size.X, windowSize.Y - size.Y - 16);
        }

        /// <summary>
        /// Returns a position vector centering a rectangle of the given size in the lower third of the screen.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        Vect2i GetThirdLowerPosition (Vect2i size) {
            Vect2i windowSize = GameAccess.Interface.WindowSize;
            float third = (windowSize.Y / 3);
            int yOffset = (int)Math.Round (2 * third + (((float)third - size.Y) / 2));

            return new Vect2i ((int)Math.Round ((decimal)(windowSize.X - size.X) / 2), yOffset);
        }

        Vect2i GetMainMenuPosition (Vect2i size) {
            Vect2i windowSize = GameAccess.Interface.WindowSize;
            float third = (windowSize.X / 3);
            int xOffset = (int)(2 * third) - (size.X / 2);
            return new Vect2i (xOffset, (int)Math.Round ((decimal)(windowSize.Y - size.Y) / 2));
        }

        /// <summary>
        /// Return a gui elements starting position.
        /// </summary>
        /// <returns>The GUI position.</returns>
        /// <param name="ident">Ident.</param>
        /// <param name="section">Section.</param>
        /// <param name="size">Size.</param>
        public Vect2i GetGuiPosition (WindowPresets setting) {
            Vect2i windowSize = GameAccess.Interface.WindowSize;

            if (setting.SavesPosition) {
                if (!_positions.ContainsKey (windowSize)) {
                    _positions [windowSize] = new Dictionary<string, Vect2i> ();
                }
                if (!_positions [windowSize].ContainsKey (setting.Key)) {
                    _positions [windowSize] [setting.Key] = GetGuiPositionDefault (setting);
                }
                return _positions [windowSize] [setting.Key];

            } else {
                return GetGuiPositionDefault (setting);
            }
        }

        /// <summary>
        /// Sets a new default GUI position.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="position">Position.</param>
        public void SetGuiPosition (WindowPresets setting, Vect2i position) {
            if (!setting.SavesPosition) {
                return;
            }

            Vect2i windowSize = GameAccess.Interface.WindowSize;

            if (!_positions.ContainsKey (windowSize)) {
                _positions [windowSize] = new Dictionary<string, Vect2i> ();
            }

            _positions [windowSize] [setting.Key] = position;
        }

        /// <summary>
        /// Gets the default (static) GUI position.
        /// </summary>
        /// <returns>The GUI position default.</returns>
        /// <param name="setting">Setting.</param>
        public Vect2i GetGuiPositionDefault (WindowPresets setting) {
            Vect2i windowSize = GameAccess.Interface.WindowSize;

            switch (setting.Positioning) {
                case Positioning.MainMenu:
                    return GetMainMenuPosition (setting.GetOuterSize (UIProvider));
                case Positioning.LowerMiddle:
                    return new Vect2i (
                        (int)Math.Round ((decimal)(windowSize.X - setting.GetOuterSize (UIProvider).X) / 2),
                        windowSize.Y - setting.GetOuterSize (UIProvider).Y - 16
                    );
                case Positioning.LowerThirdMiddle:
                    return GetThirdLowerPosition (setting.GetOuterSize (UIProvider));
                case Positioning.UpperMiddle:
                    return new Vect2i ((int)Math.Round ((decimal)(windowSize.X - setting.GetOuterSize (UIProvider).X) / 2), 0);
                case Positioning.UpperLowerMiddle:
                    return new Vect2i ((windowSize.X - setting.GetOuterSize (UIProvider).X) / 2, 72);
                case Positioning.UpperThirdMiddle:
                    return new Vect2i ((windowSize.X - setting.GetOuterSize (UIProvider).X) / 2, (windowSize.Y / 3) - setting.GetOuterSize (UIProvider).Y);
                case Positioning.LowerLeft:
                    return GetLowerLeftPosition (setting.GetOuterSize (UIProvider));
                case Positioning.LowerRight:
                    return GetLowerRightPosition (setting.GetOuterSize (UIProvider));
                case Positioning.UpperRight:
                    return GetUpperRightPosition (setting.GetOuterSize (UIProvider));
                case Positioning.UpperLeft:
                    return new Vect2i (16, 72);
                case Positioning.CenteredLeft:
                    return new Vect2i (16, (windowSize.Y - setting.GetOuterSize (UIProvider).Y) / 2);
                default:
                    return new Vect2i (
                        (int)Math.Round ((decimal)(windowSize.X - setting.GetOuterSize (UIProvider).X) / 2),
                        (int)Math.Round ((decimal)(windowSize.Y - 16 - setting.GetOuterSize (UIProvider).Y) / 2) + 34
                    );
            }
        }

        #region Saving & Loading

        string _settingsFile = GameAccess.Folders.GetFilePath (Constants.PATH_SETTINGS, "Gui.json");

        void LoadGuiPositions () {
            if (!File.Exists (_settingsFile))
                return;

            string json = File.ReadAllText (_settingsFile);
            JsonObject result = JsonParser.JsonDecode (json).GetValue<JsonObject> ();
            foreach (KeyValuePair<string, JsonNode> entry in result) {
                Vect2i section = Vect2i.Parse (entry.Key);
                if (!_positions.ContainsKey (section)) {
                    _positions [section] = new Dictionary<string, Vect2i> ();
                }

                foreach (var setting in entry.Value.AsEnumerable<JsonObject>()) {
                    _positions [section] [setting ["key"].GetValue<string> ()] = Vect2i.Parse (setting ["value"].GetValue<string> ());
                }
            }
        }

        public void SaveGuiPositions () {
            Hashtable sections = new Hashtable ();
            foreach (KeyValuePair<Vect2i, Dictionary<string, Vect2i>> section in _positions) {
                ArrayList settings = new ArrayList ();
                foreach (KeyValuePair<string, Vect2i> entry in section.Value) {
                    Hashtable setting = new Hashtable ();
                    setting.Add ("key", entry.Key);
                    setting.Add ("value", string.Format ("{0}x{1}", entry.Value.X, entry.Value.Y));
                    settings.Add (setting);
                }
                sections.Add (string.Format ("{0}x{1}", section.Key.X, section.Key.Y), settings);
            }

            string json = JsonParser.JsonEncode (sections);
            File.WriteAllText (_settingsFile, json);
        }

        #endregion

        #endregion

        #region Rendering

        /// <summary>
        /// Draws the gui to the specified RenderTarget with the specified RenderStates.
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="states">States.</param>
        public void Draw (RenderTarget target, RenderStates states) {

            // Set some vars.
            SetKeyboardState ();
            SortWindows ();

            if (MouseHeld > 0) {
                MouseHeld++;
            }

            if (NeedsWipe) {
                Wipe ();
                NeedsWipe = false;
            }

            if (_heldObject != null && _heldObject.HideMouseCursor) {
                _scene.SetMouseCursorVisible (false);
            } else {
                _scene.SetMouseCursorVisible (true);
            }

            ResetCompactCells ();
            // Draw uncompacted windows and collected compacted ones.
            foreach (GuiWindow window in SortedWindows.Reverse()) {

                // Update the element
                window.Update ();

                // Draw element.
                window.Draw (target, states);

                // Raise the OnRendered event.
                window.OnRendered ();
            }

            // Draw tooltip if any.
            if (Tooltip != null) {
                Tooltip.Update ();
                Tooltip.Draw (target, RenderStates.DEFAULT);
                Tooltip.OnRendered ();
            }

            // Draw the held object.
            _heldObject.Update ();
            _heldObject.Draw (target, RenderStates.DEFAULT);
            _heldObject.OnRendered ();

            if (_displayProfiling) {
                // Add the fps counter
                _informations [0] = string.Format ("§#ffff00§{0} Frames/s", ((int)GameAccess.Interface.FramesPerSecond).ToString ());
                _informations [1] = string.Format ("§#ffff00§{0} DrawCalls/f", (GameAccess.Interface.GLOperationsPerFrame ["DrawCalls"]).ToString ());
                _informations [2] = string.Format ("§#ffff00§{0} TextureChanges/f", (GameAccess.Interface.GLOperationsPerFrame ["TextureChanges"]).ToString ());
                _informations [3] = string.Format ("§#ffff00§{0} VBUpdates/f", GameAccess.Interface.GLOperationsPerFrame ["VBVertUpdates"].ToString ());
                _informations [4] = string.Format ("§#ffff00§{0} PushedGLStates/f", GameAccess.Interface.GLOperationsPerFrame ["PushedGLStates"].ToString ());
                _informations [5] = string.Format ("§#ffff00§{0} ViewChanges/f", GameAccess.Interface.GLOperationsPerFrame ["ViewChanges"].ToString ());
                _informations [6] = string.Format ("§#ffff00§{0} FBOChanges/f", GameAccess.Interface.GLOperationsPerFrame ["FBOChanges"].ToString ());

                int yShift = 8;

                for (int i = 0; i < _informations.Length; i++) {
                    if (_buffers [i] == null) {
                        _buffers [i] = new TextBuffer (_informations [i]);
                    } else {
                        _buffers [i].Text = _informations [i];
                    }

                    RenderStates lstates = states;
                    lstates.Transform.Translate (target.Size.X - 8 - (int)_buffers [i].LocalBounds.Size.X, yShift);
                    yShift += (int)(_buffers [i].LocalBounds.Size.Y + FontManager.Instance.Regular.LineSpacing);

                    _buffers [i].Draw (target, lstates);
                }
            }
        }

        /// <summary>
        /// Draws the map legend elements for the given layer.
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="states">States.</param>
        /// <param name="layer">Layer.</param>
        public void DrawMapLegend (RenderTarget target, RenderStates states, UILayer layer) {
            foreach (LegendElement legend in _legends.Values.Where(p => p.MapLayer == layer)) {
                // Update the element
                legend.Update ();

                // Draw element.
                legend.Draw (target, states);

                // Raise the OnRendered event.
                legend.OnRendered ();
            }

        }

        #region CompactGrid

        int _compacts = 0;
        public static readonly Vect2i COMPACT_SIZE = new Vect2i (64, 64);
        public const int GRID_EDGE = 96;

        void ResetCompactCells () {
            _compacts = 0;
        }

        public int GetNextCompactCell () {
            _compacts++;
            return _compacts - 1;
        }

        #endregion

        #endregion

        #region Theme Helper

        public Sprite GetGuiSplash (string ident) {
            return new Sprite (SpriteManager.Instance.LoadTexture ("Textures.Splashes." + ident + ".png"));
        }

        public Sprite GetGuiSprite (string ident) {
            return SpriteManager.Instance [SpriteManager.Instance.RegisterSingle (ident)];
        }

        #endregion
    }
}
