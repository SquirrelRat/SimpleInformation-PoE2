# SimpleInformation for PoE2

A clean, customizable, all-in-one information bar for **ExileCore2** (Path of Exile 2).

<img width="972" height="31" alt="image" src="https://github.com/user-attachments/assets/7a20262e-ff7e-45a8-a866-399b976d967b" />


---

> **Note:** This plugin is designed as a streamlined replacement for default information overlays.
>
> To avoid overlapping information and UI clutter, please ensure you disable any conflicting "MiscInformation" or default UI plugins in your ExileCore2 settings if applicable.

---

## Features

* **All-in-One Information Bar:** Consolidates essential data into a single, clean, horizontal bar that docks neatly at the top of the screen.

* **Key Statistics at a Glance:**
    * **Player Level:** Displays current level.
    * **Area Info:** Current Area Name (including **Map Tier** if applicable) and Time Spent in Area.
    * **Ping:** Real-time latency with color-coded signal bars.
    * **XP Tracking:** XP per Hour (XP/h) and percentage of level gained.
    * **Leveling:** Estimated Time remaining to Level Up.
    * **Gold:** Displays current **Inventory Gold** and **Stored Gold** (display format `Inv (Stored)`).
    * **Gold per Hour (G/H):** Estimates gold earnings during your current session.

* **Interactive Tooltips:** Hover over specific sections of the bar for detailed breakdowns:
    * **XP Section:** View total XP gained in the current area and the specific % of the level gained in that run.
    * **Gold Section:** View total Gold gained in the current area and the duration of the run.
    * **Ping Section:** Displays a **Live Latency Graph** history.

* **Extensive Theme Support:** Includes 10+ built-in color schemes to match your setup, plus a fully custom option.

### Available Color Schemes
* **System:** Default, Inverted, Monochrome
* **Software/IDE:** Dracula, Solarized Dark
* **Game Themes:**
    * Cyberpunk 2077
    * Overwatch
    * Minecraft
    * Valorant
    * Halo

---

## Installation

1. Download the plugin source.
2. Place the `SimpleInformationPoE2` folder into your `ExileCore2/Plugins/` directory.
3. Start (or restart) **ExileCore2**.
4. Enable the plugin in the settings menu (**F12**).

## Configuration

Open the ExileCore2 menu (Default: **F12**) and navigate to **SimpleInformation**.

* **Enable:** Toggle the plugin on/off (Default Hotkey: **F10**).
* **Color Scheme:** Select from the dropdown list of themes.
* **Modules:** Check/Uncheck boxes to display only the data you care about (e.g., `Show Gold`, `Show Ping`, `Show XP Rate`).
