using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Cache;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using JM.LinqFaster;
using Input = ExileCore2.Input;
using Vector2N = System.Numerics.Vector2;
using RectangleF = ExileCore2.Shared.RectangleF;

// Explicitly using System.Drawing for Color
using Color = System.Drawing.Color;

namespace SimpleInformationPoE2
{
    public class SimpleInformationPoE2 : BaseSettingsPlugin<SimpleInformationSettings>
    {
        private string areaName = "";

        private Dictionary<int, float> arenaEffectiveLevels = new Dictionary<int, float>();

        private TimeCache<bool> calcXp;
        private bool canRender;
        private DebugInformation debugInformation;
        private string latency = "";
        private string ping = "";
        private int pingBars;
        private Color pingBarColor;
        private string gold = "";
        private string goldPerHour = "";
        private ExileCore2.Shared.RectangleF leftPanelStartDrawRect = ExileCore2.Shared.RectangleF.Empty;
        private TimeCache<bool> levelPenalty;
        private double levelXpPenalty, partyXpPenalty;
        private float percentGot;
        private DateTime startTime;
        private long startXp, getXp, xpLeftQ;
        private DateTime lastTime;
        private string time = "";
        private string timeLeft = "";
        private TimeSpan timeSpan;
        private string xpRate = "";
        private string xpText = "";
        private string playerLevelText = "";
        private TimeSpan _timeToEmptyCountdown = TimeSpan.Zero;
        private long _lastStoredGoldForCountdown = -1;

        private long _sessionGoldGained = 0;
        private TimeSpan _sessionElapsedTime = TimeSpan.Zero;
        private long _sessionGoldSpentFromStash = 0;
        private TimeSpan _sessionStashSpendElapsedTime = TimeSpan.Zero;
        private long _lastInventoryGold = 0;
        private long _lastStoredGold = 0;
        private DateTime _lastGoldActivityTime = DateTime.UtcNow;
        private DateTime _lastStashSpendActivityTime = DateTime.UtcNow;

        private long _lastAreaGoldGained = 0;
        private double _lastAreaXpGained = 0;
        private TimeSpan _lastAreaTimeSpent = TimeSpan.Zero;
        private int _startLevel;
        private int _lastAreaStartLevel;
        private AreaInstance _previousArea;
        private readonly Dictionary<string, ExileCore2.Shared.RectangleF> _segmentBounds = new Dictionary<string, ExileCore2.Shared.RectangleF>();
        private string _lastTheme = "";

        private readonly Queue<int> _pingHistory = new Queue<int>();
        private const int MAX_HISTORY_POINTS = 15;

        private Color ToDrawingColor(Color color) => color;

        public float GetEffectiveLevel(int monsterLevel)
        {
            return Convert.ToSingle(-0.03 * Math.Pow(monsterLevel, 2) + 5.17 * monsterLevel - 144.9);
        }

        public override void OnLoad()
        {
            Order = -50;
        }

        public override bool Initialise()
        {
            Input.RegisterKey(Keys.F10);

            Input.ReleaseKey += (sender, keys) =>
            {
                if (keys == Keys.F10) Settings.Enable.Value = !Settings.Enable;
            };

            GameController.LeftPanel.WantUse(() => ((ExileCore2.Shared.Interfaces.ISettings)Settings).Enable);
            calcXp = new TimeCache<bool>(() =>
            {
                var gameUi = GameController.Game.IngameState.IngameUi;
                if (GameController.Area.CurrentArea == null || gameUi.InventoryPanel.IsVisible)
                {
                    canRender = false;
                    return false;
                }

                var UIHover = GameController.Game.IngameState.UIHover;
                if (UIHover.Tooltip != null && UIHover.Tooltip.IsVisibleLocal &&
                    UIHover.Tooltip.GetClientRectCache.Intersects(leftPanelStartDrawRect))
                {
                    canRender = false;
                    return false;
                }

                canRender = true;

                var now = DateTime.UtcNow;
                var deltaTime = now - lastTime;
                lastTime = now;

                UpdateLevelAndXp();
                UpdateGeneralInfo();
                UpdateGoldStats(deltaTime);

                var levelPenaltyValue = levelPenalty.Value;

                return true;
            }, 1000);

            levelPenalty = new TimeCache<bool>(() =>
            {
                partyXpPenalty = PartyXpPenalty();
                levelXpPenalty = LevelXpPenalty();
                return true;
            }, 1000);

            OnEntityListWrapperOnPlayerUpdate(this, GameController.Player);

            Settings.ResetThemeColors.OnPressed += ResetThemeColors;

            UpdateColorNodes();

            return true;
        }

        private void OnEntityListWrapperOnPlayerUpdate(object sender, Entity entity)
        {
            var player = GameController?.Player;
            var playerComp = player?.GetComponent<Player>();
            if (player == null || playerComp == null) return;

            percentGot = 0;
            xpRate = "0.00 xp/h";
            var level = playerComp.Level;
            playerLevelText = $"Lvl: {level}";
            timeLeft = "-h -m -s to Level Up";
            getXp = 0;
            xpLeftQ = 0;
            xpText = "";

            startTime = lastTime = DateTime.UtcNow;
            startXp = playerComp.XP;
            levelXpPenalty = LevelXpPenalty();
            _startLevel = level;
        }

        public override void AreaChange(AreaInstance area)
        {
            _lastStoredGoldForCountdown = -1;
            levelPenalty.ForceUpdate();

            var player = GameController?.Player;
            var playerComp = player?.GetComponent<Player>();

            if (_previousArea != null && !_previousArea.IsHideout && !_previousArea.IsTown)
            {
                _lastAreaGoldGained = _sessionGoldGained;
                if (playerComp != null)
                    _lastAreaXpGained = playerComp.XP - startXp;
                _lastAreaTimeSpent = timeSpan;
                _lastAreaStartLevel = _startLevel;
            }

            _sessionGoldGained = 0;
            _sessionElapsedTime = TimeSpan.Zero;
            _sessionGoldSpentFromStash = 0;
            _sessionStashSpendElapsedTime = TimeSpan.Zero;

            var currentGold = GameController.Game.IngameState.ServerData.Gold;
            var storedGold = 0;
            _lastInventoryGold = currentGold;
            _lastStoredGold = storedGold;
            _lastGoldActivityTime = DateTime.UtcNow;

            startTime = lastTime = DateTime.UtcNow;
            if (playerComp != null)
            {
                startXp = playerComp.XP;
                _startLevel = playerComp.Level;
            }
            getXp = 0;
        }

        private void CalculateXp()
        {
            var player = GameController?.Player;
            var playerComp = player?.GetComponent<Player>();
            var level = playerComp?.Level ?? 100;

            if (level >= 100)
            {
                xpRate = "0.00 xp/h";
                timeLeft = "--h--m--s";
                return;
            }

            if (playerComp == null) return;

            long currentXp = playerComp.XP;
            getXp = currentXp - startXp;
            var rate = (currentXp - startXp) / (DateTime.UtcNow - startTime).TotalHours;
            xpRate = $"{ConvertHelper.ToShorten(rate, "0.00")} xp/h";

            if (level >= 0 && level + 1 < Constants.PlayerXpLevels.Length && rate > 1)
            {
                var xpLeft = Constants.PlayerXpLevels[level + 1] - currentXp;
                xpLeftQ = xpLeft;
                var time = TimeSpan.FromHours(xpLeft / rate);
                timeLeft = $"Lvl Up: {time.Hours:0}h {time.Minutes:00}m {time.Seconds:00}s";

                if (getXp == 0)
                    percentGot = 0;
                else
                {
                    percentGot = getXp / ((float) Constants.PlayerXpLevels[level + 1] - (float) Constants.PlayerXpLevels[level]);
                    if (percentGot < -100) percentGot = 0;
                }
            }
        }

        private double LevelXpPenalty()
        {
            var arenaLevel = GameController.Area.CurrentArea.RealLevel;
            var characterLevel = GameController?.Player?.GetComponent<Player>()?.Level ?? 100;


            if (arenaLevel > 70 && !arenaEffectiveLevels.ContainsKey(arenaLevel))
            {
                arenaEffectiveLevels.Add(arenaLevel, GetEffectiveLevel(arenaLevel));
            }
            var effectiveArenaLevel = arenaLevel < 71 ? arenaLevel : arenaEffectiveLevels[arenaLevel];
            var safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            var effectiveDifference = Math.Max(Math.Abs(characterLevel - effectiveArenaLevel) - safeZone, 0);
            double xpMultiplier;

            xpMultiplier = Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5);

            if (characterLevel >= 95) 
                xpMultiplier *= 1d / (1 + 0.1 * (characterLevel - 94));

            xpMultiplier = Math.Max(xpMultiplier, 0.01);

            return xpMultiplier;
        }

        private double PartyXpPenalty()
        {
            var entities = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Player];

            if (entities.Count == 0)
                return 1;

            var levels = entities.Select(y => y.GetComponent<Player>()?.Level ?? 100).ToList();
            var characterLevel = GameController?.Player?.GetComponent<Player>()?.Level ?? 100;
            var partyXpPenalty = Math.Pow(characterLevel + 10, 2.71) / levels.SumF(level => Math.Pow(level + 10, 2.71));
            return partyXpPenalty * entities.Count;
        }

        private void UpdateLevelAndXp()
        {
            var level = GameController.Player.GetComponent<Player>()?.Level ?? 100;
            playerLevelText = $"Lvl: {level}";
            if (level != _startLevel)
            {
                OnEntityListWrapperOnPlayerUpdate(this, GameController.Player);
            }
            CalculateXp();
        }

        private void UpdateGeneralInfo()
        {
            var areaCurrentArea = GameController.Area.CurrentArea;
            timeSpan = DateTime.UtcNow - areaCurrentArea.TimeEntered;
            time = AreaInstance.GetTimeString(timeSpan);
            xpText = $"{xpRate} ({percentGot:P0})".ToUpper();

            var areaSuffix = (areaCurrentArea.RealLevel >= 68) ? $" - T{areaCurrentArea.RealLevel - 67}" : "";
            areaName = $"{areaCurrentArea.DisplayName}{areaSuffix}";
            ping = $"Ping: {GameController.Game.IngameState.ServerData.Latency}";

            var latency = GameController.Game.IngameState.ServerData.Latency;
            if (latency < 50) { pingBars = 4; pingBarColor = Color.Green; }
            else if (latency <= 100) { pingBars = 3; pingBarColor = Color.Green; }
            else if (latency <= 150) { pingBars = 2; pingBarColor = Color.Yellow; }
            else { pingBars = 1; pingBarColor = Color.Red; }

            _pingHistory.Enqueue(latency);
            if (_pingHistory.Count > MAX_HISTORY_POINTS)
                _pingHistory.Dequeue();
        }

        private void UpdateGoldStats(TimeSpan deltaTime)
        {
            if (!Settings.ShowGoldPerHour.Value && !Settings.ShowGold.Value)
            {
                goldPerHour = "";
                gold = "";
                _sessionGoldGained = 0;
                _sessionElapsedTime = TimeSpan.Zero;
                _sessionGoldSpentFromStash = 0;
                _sessionStashSpendElapsedTime = TimeSpan.Zero;
                var currentGoldOnReset = GameController.Game.IngameState.ServerData.Gold;
                var storedGoldOnReset = 0;
                _lastInventoryGold = currentGoldOnReset;
                _lastStoredGold = storedGoldOnReset;
                _lastGoldActivityTime = DateTime.UtcNow;
                return;
            }

            var currentGold = GameController.Game.IngameState.ServerData.Gold;
            var storedGold = 0;

            var totalWagePerHour = 0;
            _timeToEmptyCountdown = TimeSpan.Zero;

            if (GameController.Area.CurrentArea.IsHideout || GameController.Area.CurrentArea.IsTown)
            {
                goldPerHour = "G/H: N/A";
            }
            else
            {
                if (!GameController.Area.CurrentArea.IsPeaceful && !IsAnyGameUIVisible())
                {
                    _sessionElapsedTime += deltaTime;
                }

                var inventoryGoldDifference = currentGold - _lastInventoryGold;
                if (inventoryGoldDifference > 0)
                {
                    _sessionGoldGained += inventoryGoldDifference;
                    _lastGoldActivityTime = DateTime.UtcNow;
                }
                _lastInventoryGold = currentGold;

                var storedGoldDifference = storedGold - _lastStoredGold;
                if (storedGoldDifference < 0)
                {
                    _sessionGoldSpentFromStash += Math.Abs(storedGoldDifference);
                    _sessionStashSpendElapsedTime += deltaTime;
                    _lastGoldActivityTime = DateTime.UtcNow;
                    _lastStashSpendActivityTime = DateTime.UtcNow;
                }
                else if (DateTime.UtcNow - _lastStashSpendActivityTime > TimeSpan.FromSeconds(5))
                {
                    _sessionGoldSpentFromStash = 0;
                    _sessionStashSpendElapsedTime = TimeSpan.Zero;
                }
                _lastStoredGold = storedGold;

                if (DateTime.UtcNow - _lastGoldActivityTime > TimeSpan.FromMinutes(5))
                {
                    _sessionGoldGained = 0;
                    _sessionElapsedTime = TimeSpan.Zero;
                    _sessionGoldSpentFromStash = 0;
                    _sessionStashSpendElapsedTime = TimeSpan.Zero;
                    _lastInventoryGold = currentGold;
                    _lastStoredGold = storedGold;
                    _lastGoldActivityTime = DateTime.UtcNow;
                }

                var sessionGainRate = _sessionElapsedTime.TotalHours > 0 ? (_sessionGoldGained - _sessionGoldSpentFromStash) / _sessionElapsedTime.TotalHours : 0;
                goldPerHour = $"G/H: {FormatNumber(sessionGainRate)}";
            }

            if (Settings.ShowGold.Value)
            {
                gold = $"Gold: {FormatNumber(currentGold)} ({FormatNumber(storedGold)})";
            }
        }

        public override void Render()
        {
            if (calcXp == null) return;

            if (Settings.ColorScheme.Value != _lastTheme)
            {
                UpdateColorNodes();
                _lastTheme = Settings.ColorScheme.Value;
            }

            var dummy = calcXp.Value;
            
            if (GameController.Area == null) return;

            _previousArea = GameController.Area.CurrentArea;
            if (!canRender)
                return;

            var origStartPoint = GameController.LeftPanel.StartDrawPoint;
            var colorScheme = GetColorScheme();
            var items = new List<(string text, Color color, bool isPing, string id)>();
            _segmentBounds.Clear();

            if (Settings.ShowPlayerLevel.Value)
                items.Add((playerLevelText, colorScheme.XphGetLeft, false, "PlayerLevel"));
            if (Settings.ShowAreaName.Value)
                items.Add((areaName, colorScheme.Area, false, "AreaName"));
            if (Settings.ShowAreaTime.Value)
                items.Add((time, colorScheme.Timer, false, "AreaLeft"));
            if (Settings.ShowTimeLeft.Value)
                items.Add((timeLeft, colorScheme.TimeLeft, false, "TimeLeft"));
            if (Settings.ShowXpRate.Value)
                items.Add((xpText, colorScheme.Xph, false, "XpRate"));
            if (Settings.ShowGold.Value)
                items.Add((gold, colorScheme.Timer, false, "Gold"));
            if (Settings.ShowGoldPerHour.Value)
                items.Add((goldPerHour, colorScheme.Timer, false, "GoldPerHour"));
            if (Settings.ShowPing.Value)
                items.Add((ping, colorScheme.Ping, true, "Ping"));

            if (items.Count == 0)
            {
                GameController.LeftPanel.StartDrawPoint = new Vector2N(origStartPoint.X, origStartPoint.Y);
                return;
            }

            var horizontalPadding = 15;
            var verticalPadding = 5;
            var totalTextWidth = 0f;
            var separatorWidth = Graphics.MeasureText(" | ").X;

            foreach (var (text, color, isPing, id) in items)
            {
                if (isPing)
                {
                    totalTextWidth += Graphics.MeasureText("Ping: " + GameController.Game.IngameState.ServerData.Latency).X + 5 + 4 * 6;
                }
                else
                {
                    totalTextWidth += Graphics.MeasureText(text).X;
                }
                totalTextWidth += separatorWidth;
            }
            totalTextWidth -= separatorWidth;

            var barWidth = totalTextWidth + horizontalPadding * 2;
            var maxHeight = items.Max(x => Graphics.MeasureText(x.text).Y);

            var drawPoint = new Vector2N(
                (GameController.Window.GetWindowRectangle().Width - barWidth) / 2 + Settings.DrawXOffset.Value,
                origStartPoint.Y);
            leftPanelStartDrawRect = new RectangleF(drawPoint.X, drawPoint.Y, 1, 1);

            var bounds = new RectangleF(drawPoint.X, drawPoint.Y, barWidth, maxHeight + verticalPadding * 2);
            var backgroundColor = Color.FromArgb((byte)Settings.BackgroundAlpha.Value, colorScheme.Background.R, colorScheme.Background.G, colorScheme.Background.B);
            Graphics.DrawBox(bounds, backgroundColor);

            var textDrawPoint = new Vector2N(drawPoint.X + horizontalPadding, drawPoint.Y + verticalPadding);

            for (int i = 0; i < items.Count; i++)
            {
                var (text, color, isPing, id) = items[i];
                var startX = textDrawPoint.X;

                if (isPing)
                {
                    var pingTextSize = Graphics.DrawText(text, textDrawPoint, ToDrawingColor(colorScheme.Ping));
                    textDrawPoint.X += pingTextSize.X + 5;

                    for (int j = 0; j < 4; j++)
                    {
                        var barColor = j < pingBars ? pingBarColor : Color.Gray;
                        var barBounds = new RectangleF(textDrawPoint.X + j * 6, textDrawPoint.Y + maxHeight / 4, 5, maxHeight / 2);
                        Graphics.DrawBox(barBounds, barColor);
                    }
                    textDrawPoint.X += 4 * 6;
                }
                else
                {
                    var textSize = Graphics.DrawText(text, textDrawPoint, ToDrawingColor(color));
                    textDrawPoint.X += textSize.X;
                }

                var segmentWidth = textDrawPoint.X - startX;
                _segmentBounds[id] = new RectangleF(startX, drawPoint.Y, segmentWidth, maxHeight + verticalPadding * 2);

                if (i < items.Count - 1)
                {
                    var separatorSize = Graphics.DrawText(" | ", textDrawPoint, ToDrawingColor(colorScheme.Timer));
                    textDrawPoint.X += separatorSize.X;
                }
            }

            if (_segmentBounds.ContainsKey("Gold") && _segmentBounds.ContainsKey("GoldPerHour"))
            {
                var goldBounds = _segmentBounds["Gold"];
                var ghBounds = _segmentBounds["GoldPerHour"];
                var combinedX = Math.Min(goldBounds.X, ghBounds.X);
                var combinedWidth = Math.Max(goldBounds.X + goldBounds.Width, ghBounds.X + ghBounds.Width) - combinedX;
                _segmentBounds["Gold"] = new RectangleF(combinedX, goldBounds.Y, combinedWidth, goldBounds.Height);
                _segmentBounds.Remove("GoldPerHour");
            }

            HandleTooltips(drawPoint, maxHeight, verticalPadding, colorScheme);

            GameController.LeftPanel.StartDrawPoint = new Vector2N(origStartPoint.X, origStartPoint.Y + maxHeight + 10);
        }

        private void HandleTooltips(Vector2N drawPoint, float maxHeight, float verticalPadding, ColorScheme colorScheme)
        {
            foreach (var segment in _segmentBounds)
            {
                if (segment.Value.Contains(new Vector2N(Input.MousePosition.X, Input.MousePosition.Y)))
                {
                    var tooltipParts = new List<(string text, Color color)>();
                    switch (segment.Key)
                    {
                        case "XpRate":
                            tooltipParts.Add(($"XP Gained in Area: {FormatNumber(_lastAreaXpGained)}", colorScheme.Xph));
                            if (_lastAreaStartLevel > 0 && _lastAreaStartLevel < 100)
                            {
                                var totalXpForLevel = (float)Constants.PlayerXpLevels[_lastAreaStartLevel + 1] - (float)Constants.PlayerXpLevels[_lastAreaStartLevel];
                                if (totalXpForLevel > 0)
                                {
                                    var percentGained = _lastAreaXpGained / totalXpForLevel;
                                    tooltipParts.Add(($" ({percentGained:P1} of Lvl {_lastAreaStartLevel})", colorScheme.Xph));
                                }
                            }
                            break;
                        case "GoldPerHour":
                        case "Gold":
                            tooltipParts.Add(($"Gold Gained in Area: {FormatNumber(_lastAreaGoldGained)}", colorScheme.Timer));
                            tooltipParts.Add(($" | Time: {_lastAreaTimeSpent:mm\\:ss}", colorScheme.Timer));
                            if (_timeToEmptyCountdown > TimeSpan.Zero)
                            {
                                string emptyInFormatted;
                                if (_timeToEmptyCountdown.Days > 0)
                                {
                                    string dayText = _timeToEmptyCountdown.Days == 1 ? "Day" : "Days";
                                    emptyInFormatted = $"{_timeToEmptyCountdown.Days} {dayText}, {_timeToEmptyCountdown.Hours:00}:{_timeToEmptyCountdown.Minutes:00}:{_timeToEmptyCountdown.Seconds:00}";
                                }
                                else
                                {
                                    emptyInFormatted = $"{_timeToEmptyCountdown.Hours:00}:{_timeToEmptyCountdown.Minutes:00}:{_timeToEmptyCountdown.Seconds:00}";
                                }
                                tooltipParts.Add((" | Village Gold Empty In: " + emptyInFormatted, colorScheme.Timer));
                            }
                            break;
                        case "Ping":
                            break;
                    }

                    bool hasGraph = segment.Key == "Ping";
                    if (tooltipParts.Count > 0 || hasGraph)
                    {
                        DrawTooltip(tooltipParts, segment.Value, maxHeight, verticalPadding, colorScheme, hasGraph);
                        return;
                    }
                }
            }
        }
        private void DrawTooltip(List<(string text, Color color)> tooltipParts, ExileCore2.Shared.RectangleF segmentBounds, float maxHeight, float verticalPadding, ColorScheme colorScheme, bool hasGraph)
        {
            var totalWidth = tooltipParts.Any() ? tooltipParts.Sum(p => Graphics.MeasureText(p.text).X) + 30 : 100;
            var tooltipMaxHeight = tooltipParts.Any() ? tooltipParts.Max(p => Graphics.MeasureText(p.text).Y) : 0;
            var overlayHeight = tooltipMaxHeight + 10;

            if (hasGraph)
            {
                overlayHeight += 25;
            }

            var overlayPosition = new Vector2N(
                segmentBounds.X + (segmentBounds.Width - totalWidth) / 2,
                segmentBounds.Y + segmentBounds.Height + 5);

            Graphics.DrawBox(new RectangleF(overlayPosition.X, overlayPosition.Y, totalWidth, overlayHeight), colorScheme.Background);

            if (tooltipParts.Any())
            {
                var textDrawPos = overlayPosition + new Vector2N(15, 5);
                foreach (var part in tooltipParts)
                {
                    var size = Graphics.DrawText(part.text, textDrawPos, ToDrawingColor(part.color));
                    textDrawPos.X += size.X;
                }
            }

            if (hasGraph)
            {
                DrawMiniGraph(_pingHistory, overlayPosition, totalWidth, colorScheme.Ping);
            }
        }

        private void DrawMiniGraph(Queue<int> data, Vector2N overlayPosition, float totalWidth, Color graphColor)
        {
            if (data.Count < 2) return;

            var graphX = overlayPosition.X + 5;
            var graphY = overlayPosition.Y + 5;
            var graphWidth = totalWidth - 10;
            var graphHeight = 15;

            var values = data.ToArray();
            var min = (double)values.Min();
            var max = (double)values.Max();
            var range = max - min;
            if (range == 0) range = 1;

            var points = new List<System.Numerics.Vector2>();
            for (int i = 0; i < values.Length; i++)
            {
                var x = graphX + (i * graphWidth / (values.Length - 1));
                var y = graphY + graphHeight - ((values[i] - min) / range * graphHeight);
                points.Add(new System.Numerics.Vector2(x, (float)y));
            }

            for (int i = 1; i < points.Count; i++)
            {
                Graphics.DrawLine(points[i - 1], points[i], 2f, graphColor);
            }
        }

        private bool IsAnyGameUIVisible()
        {
            var ui = GameController.IngameState.IngameUi;
            return ui.InventoryPanel.IsVisible ||
                   ui.OpenLeftPanel.IsVisible ||
                   ui.TreePanel.IsVisible ||
                   ui.ExpeditionWindow.IsVisible ||
                   ui.RitualWindow.IsVisible ||
                   ui.UltimatumPanel.IsVisible;
        }

        private void UpdateColorNodes()
        {
            var scheme = (ColorSchemeList)Enum.Parse(typeof(ColorSchemeList), Settings.ColorScheme.Value);
            if (scheme == ColorSchemeList.Custom) return;
            ColorScheme colorScheme = scheme switch
            {
                ColorSchemeList.SolarizedDark => new SolarizedDarkColorScheme(),
                ColorSchemeList.Dracula => new DraculaColorScheme(),
                ColorSchemeList.Inverted => new InvertedColorScheme(),
                ColorSchemeList.Cyberpunk2077 => new Cyberpunk2077ColorScheme(),
                ColorSchemeList.Overwatch => new OverwatchColorScheme(),
                ColorSchemeList.Minecraft => new MinecraftColorScheme(),
                ColorSchemeList.Valorant => new ValorantColorScheme(),
                ColorSchemeList.Halo => new HaloColorScheme(),
                ColorSchemeList.Monochrome => new MonochromeColorScheme(),
                _ => new DefaultColorScheme(),
            };
            Settings.BackgroundColor.Value = ToDrawingColor(colorScheme.Background);
            Settings.TimerColor.Value = ToDrawingColor(colorScheme.Timer);
            Settings.PingColor.Value = ToDrawingColor(colorScheme.Ping);
            Settings.AreaColor.Value = ToDrawingColor(colorScheme.Area);
            Settings.TimeLeftColor.Value = ToDrawingColor(colorScheme.TimeLeft);
            Settings.XphColor.Value = ToDrawingColor(colorScheme.Xph);
            Settings.XphGetLeftColor.Value = ToDrawingColor(colorScheme.XphGetLeft);
        }

        private void ResetThemeColors()
        {
            var scheme = (ColorSchemeList)Enum.Parse(typeof(ColorSchemeList), Settings.ColorScheme.Value);
            ColorScheme defaultScheme;
            if (scheme == ColorSchemeList.Custom)
            {
                Settings.BackgroundColor.Value = Color.White;
                Settings.TimerColor.Value = Color.White;
                Settings.PingColor.Value = Color.White;
                Settings.AreaColor.Value = Color.White;
                Settings.TimeLeftColor.Value = Color.White;
                Settings.XphColor.Value = Color.White;
                Settings.XphGetLeftColor.Value = Color.White;
                return;
            }
            defaultScheme = scheme switch
            {
                ColorSchemeList.SolarizedDark => new SolarizedDarkColorScheme(),
                ColorSchemeList.Dracula => new DraculaColorScheme(),
                ColorSchemeList.Inverted => new InvertedColorScheme(),
                ColorSchemeList.Cyberpunk2077 => new Cyberpunk2077ColorScheme(),
                ColorSchemeList.Overwatch => new OverwatchColorScheme(),
                ColorSchemeList.Minecraft => new MinecraftColorScheme(),
                ColorSchemeList.Valorant => new ValorantColorScheme(),
                ColorSchemeList.Halo => new HaloColorScheme(),
                ColorSchemeList.Monochrome => new MonochromeColorScheme(),
                _ => new DefaultColorScheme(),
            };
            Settings.BackgroundColor.Value = ToDrawingColor(defaultScheme.Background);
            Settings.TimerColor.Value = ToDrawingColor(defaultScheme.Timer);
            Settings.PingColor.Value = ToDrawingColor(defaultScheme.Ping);
            Settings.AreaColor.Value = ToDrawingColor(defaultScheme.Area);
            Settings.TimeLeftColor.Value = ToDrawingColor(defaultScheme.TimeLeft);
            Settings.XphColor.Value = ToDrawingColor(defaultScheme.Xph);
            Settings.XphGetLeftColor.Value = ToDrawingColor(defaultScheme.XphGetLeft);
        }

        private string FormatNumber(double num)
        {
            if (num >= 1000000000)
                return (num / 1000000000D).ToString("0.#") + "b";
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.#") + "m";
            if (num >= 1000)
                return (num / 1000D).ToString("0.#") + "k";

            return num.ToString("N0");
        }

        private ColorScheme GetColorScheme()
        {
            return new CustomColorScheme(Settings);
        }
    }
}
