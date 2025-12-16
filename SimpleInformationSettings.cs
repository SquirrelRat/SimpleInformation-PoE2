using System;
using System.Collections.Generic;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using ExileCore2.Shared.Attributes;
using SharpDX;

namespace SimpleInformationPoE2
{
    public enum ColorSchemeList
    {
        Default,
        SolarizedDark,
        Dracula,
        Inverted,
        Cyberpunk2077,
        Overwatch,
        Minecraft,
        Valorant,
        Halo,
        Monochrome,
        Custom
    }

    public class SimpleInformationSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);
        [Menu("Draw X Offset", "Adjusts the horizontal position of the information display.")]
        public RangeNode<int> DrawXOffset { get; set; } = new RangeNode<int>(0, -150, 150);
        [Menu("Color Scheme", "Selects the color scheme for the information display.")]
        public ListNode ColorScheme { get; set; } = new ListNode
        {
            Value = "Dracula",
            Values = new List<string>(Enum.GetNames(typeof(ColorSchemeList)))
        };
        [Menu("Background Alpha", "Controls the transparency of the background of the information bar (0-255).")]
        public RangeNode<int> BackgroundAlpha { get; set; } = new RangeNode<int>(150, 0, 255);
        [Menu("Show Gold", "Toggles the display of the player's gold amount.")]
        public ToggleNode ShowGold { get; set; } = new ToggleNode(true);
        [Menu("Show Player Level", "Toggles the display of the player's level.")]
        public ToggleNode ShowPlayerLevel { get; set; } = new ToggleNode(true);
        [Menu("Show Area Time", "Toggles the display of the time spent in the current area.")]
        public ToggleNode ShowAreaTime { get; set; } = new ToggleNode(true);
        [Menu("Show Ping", "Toggles the display of the player's ping.")]
        public ToggleNode ShowPing { get; set; } = new ToggleNode(true);
        [Menu("Show Area Name", "Toggles the display of the current area's name.")]
        public ToggleNode ShowAreaName { get; set; } = new ToggleNode(true);
        [Menu("Show Time Left to Level", "Toggles the display of the estimated time left to level up.")]
        public ToggleNode ShowTimeLeft { get; set; } = new ToggleNode(true);
        [Menu("Show XP Rate", "Toggles the display of the player's experience gain rate.")]
        public ToggleNode ShowXpRate { get; set; } = new ToggleNode(true);
        [Menu("Show G/H", "Toggles the display of the player's gold per hour.")]
        public ToggleNode ShowGoldPerHour { get; set; } = new ToggleNode(true);

        [Menu("Background Color", "Custom color for the background.")]
        public ColorNode BackgroundColor { get; set; } = new ColorNode((uint)(SharpDX.Color.White.A << 24 | SharpDX.Color.White.R << 16 | SharpDX.Color.White.G << 8 | SharpDX.Color.White.B));
        [Menu("Timer Color", "Custom color for timer elements.")]
        public ColorNode TimerColor { get; set; } = new ColorNode((uint)(SharpDX.Color.White.A << 24 | SharpDX.Color.White.R << 16 | SharpDX.Color.White.G << 8 | SharpDX.Color.White.B));
        [Menu("Ping Color", "Custom color for ping display.")]
        public ColorNode PingColor { get; set; } = new ColorNode((uint)(SharpDX.Color.White.A << 24 | SharpDX.Color.White.R << 16 | SharpDX.Color.White.G << 8 | SharpDX.Color.White.B));
        [Menu("Area Color", "Custom color for area name.")]
        public ColorNode AreaColor { get; set; } = new ColorNode((uint)(SharpDX.Color.White.A << 24 | SharpDX.Color.White.R << 16 | SharpDX.Color.White.G << 8 | SharpDX.Color.White.B));
        [Menu("Time Left Color", "Custom color for time left to level.")]
        public ColorNode TimeLeftColor { get; set; } = new ColorNode((uint)(SharpDX.Color.White.A << 24 | SharpDX.Color.White.R << 16 | SharpDX.Color.White.G << 8 | SharpDX.Color.White.B));
        [Menu("XP Rate Color", "Custom color for XP rate.")]
        public ColorNode XphColor { get; set; } = new ColorNode((uint)(SharpDX.Color.White.A << 24 | SharpDX.Color.White.R << 16 | SharpDX.Color.White.G << 8 | SharpDX.Color.White.B));
        [Menu("Player Level Color", "Custom color for player level.")]
        public ColorNode XphGetLeftColor { get; set; } = new ColorNode((uint)(SharpDX.Color.White.A << 24 | SharpDX.Color.White.R << 16 | SharpDX.Color.White.G << 8 | SharpDX.Color.White.B));

        [Menu("Reset Theme Colors", "Resets custom colors to the selected theme's defaults.")]
        public ButtonNode ResetThemeColors { get; set; } = new ButtonNode();
    }
}