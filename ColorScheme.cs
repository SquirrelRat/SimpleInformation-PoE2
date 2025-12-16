using System.Drawing;

namespace SimpleInformationPoE2
{
    public abstract class ColorScheme
    {
        public abstract Color Background { get; }
        public abstract Color Timer { get; }
        public abstract Color Ping { get; }
        public abstract Color Area { get; }
        public abstract Color TimeLeft { get; }
        public abstract Color Xph { get; }
        public abstract Color XphGetLeft { get; }
    }

    public class DefaultColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(255, 0, 0, 0);
        public override Color Timer => Color.FromArgb(255, 220, 190, 130);
        public override Color Ping => Color.FromArgb(255, 220, 190, 130);
        public override Color Area => Color.FromArgb(255, 140, 200, 255);
        public override Color TimeLeft => Color.FromArgb(255, 220, 190, 130);
        public override Color Xph => Color.FromArgb(255, 220, 190, 130);
        public override Color XphGetLeft => Color.FromArgb(255, 220, 190, 130);
    }


    public class SolarizedDarkColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(0xff, 0x00, 0x2b, 0x36);
        public override Color Timer => Color.FromArgb(0xff, 0x26, 0x8b, 0xd2);
        public override Color Ping => Color.FromArgb(0xff, 0x85, 0x99, 0x00);
        public override Color Area => Color.FromArgb(0xff, 0xcb, 0x4b, 0x16);
        public override Color TimeLeft => Color.FromArgb(0xff, 0xdc, 0x32, 0x2f);
        public override Color Xph => Color.FromArgb(0xff, 0xd3, 0x36, 0x82);
        public override Color XphGetLeft => Color.FromArgb(0xff, 0x6c, 0x71, 0xc4);
    }

    public class DraculaColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(0xff, 0x28, 0x2a, 0x36);
        public override Color Timer => Color.FromArgb(0xff, 0xbd, 0x93, 0xf9);
        public override Color Ping => Color.FromArgb(0xff, 0x50, 0xfa, 0x7b);
        public override Color Area => Color.FromArgb(0xff, 0xff, 0xb8, 0x6c);
        public override Color TimeLeft => Color.FromArgb(0xff, 0xff, 0x55, 0x55);
        public override Color Xph => Color.FromArgb(0xff, 0xff, 0x79, 0xc6);
        public override Color XphGetLeft => Color.FromArgb(0xff, 0x8b, 0xe9, 0xfd);
    }


    public class InvertedColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(0xff, 0xE0, 0xE0, 0xE0);
        public override Color Timer => Color.Black;
        public override Color Ping => Color.Black;
        public override Color Area => Color.Black;
        public override Color TimeLeft => Color.Black;
        public override Color Xph => Color.Black;
        public override Color XphGetLeft => Color.Black;
    }

    public class Cyberpunk2077ColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(255, 30, 45, 60);
        public override Color Timer => Color.FromArgb(255, 255, 97, 89);
        public override Color Ping => Color.FromArgb(255, 255, 97, 89);
        public override Color Area => Color.FromArgb(255, 76, 176, 165);
        public override Color TimeLeft => Color.FromArgb(255, 255, 97, 89);
        public override Color Xph => Color.FromArgb(255, 255, 97, 89);
        public override Color XphGetLeft => Color.FromArgb(255, 255, 97, 89);
    }

    // New UI-inspired color schemes (10 total)
    public class OverwatchColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(255, 18, 22, 41);
        public override Color Timer => Color.FromArgb(255, 30, 144, 255);
        public override Color Ping => Color.FromArgb(255, 30, 144, 255);
        public override Color Area => Color.FromArgb(255, 0, 191, 255);
        public override Color TimeLeft => Color.FromArgb(255, 0, 214, 144);
        public override Color Xph => Color.FromArgb(255, 255, 215, 0);
        public override Color XphGetLeft => Color.FromArgb(255, 32, 178, 170);
    }

    public class MinecraftColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(255, 20, 40, 20);
        public override Color Timer => Color.FromArgb(255, 76, 175, 80);
        public override Color Ping => Color.FromArgb(255, 76, 175, 80);
        public override Color Area => Color.FromArgb(255, 102, 187, 106);
        public override Color TimeLeft => Color.FromArgb(255, 255, 153, 51);
        public override Color Xph => Color.FromArgb(255, 255, 223, 0);
        public override Color XphGetLeft => Color.FromArgb(255, 139, 195, 74);
    }

    public class ValorantColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(255, 25, 25, 60);
        public override Color Timer => Color.FromArgb(255, 199, 0, 255);
        public override Color Ping => Color.FromArgb(255, 199, 0, 255);
        public override Color Area => Color.FromArgb(255, 111, 45, 168);
        public override Color TimeLeft => Color.FromArgb(255, 255, 0, 128);
        public override Color Xph => Color.FromArgb(255, 0, 255, 170);
        public override Color XphGetLeft => Color.FromArgb(255, 255, 204, 255);
    }

    public class HaloColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(255, 20, 20, 20);
        public override Color Timer => Color.FromArgb(255, 0, 153, 255);
        public override Color Ping => Color.FromArgb(255, 0, 153, 255);
        public override Color Area => Color.FromArgb(255, 0, 122, 204);
        public override Color TimeLeft => Color.FromArgb(255, 0, 255, 255);
        public override Color Xph => Color.FromArgb(255, 255, 215, 0);
        public override Color XphGetLeft => Color.FromArgb(255, 135, 206, 250);
    }

    public class MonochromeColorScheme : ColorScheme
    {
        public override Color Background => Color.FromArgb(255, 24, 24, 24);
        public override Color Timer => Color.Gray;
        public override Color Ping => Color.Gray;
        public override Color Area => Color.FromArgb(255, 170, 170, 170);
        public override Color TimeLeft => Color.White;
        public override Color Xph => Color.FromArgb(255, 200, 200, 200);
        public override Color XphGetLeft => Color.White;
    }

    public class CustomColorScheme : ColorScheme
    {
        private readonly SimpleInformationSettings _settings;

        public CustomColorScheme(SimpleInformationSettings settings)
        {
            _settings = settings;
        }

        public override Color Background => _settings.BackgroundColor.Value;
        public override Color Timer => _settings.TimerColor.Value;
        public override Color Ping => _settings.PingColor.Value;
        public override Color Area => _settings.AreaColor.Value;
        public override Color TimeLeft => _settings.TimeLeftColor.Value;
        public override Color Xph => _settings.XphColor.Value;
        public override Color XphGetLeft => _settings.XphGetLeftColor.Value;
    }
}