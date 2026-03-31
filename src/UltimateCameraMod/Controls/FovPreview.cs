using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UltimateCameraMod.Controls;

public class FovPreview : Canvas
{
    private const double W = 420, H = 270;

    private static readonly Brush BgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e1e"));
    private static readonly Brush CharBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c8a24e"));
    private static readonly Brush CamBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aaaaaa"));
    private static readonly Brush ConeBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x5a, 0x48, 0x20));
    private static readonly Brush ConeLineBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7a6830"));
    private static readonly Brush LabelBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
    private static readonly Brush GroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a4a4a"));
    private static readonly Brush DimBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));

    private int _fov = 25;
    private double _roff;
    private double _dist = 5.0;
    private bool _centered;

    public FovPreview()
    {
        Width = W; Height = H;
        Background = BgBrush;
        ClipToBounds = true;
        Redraw();
    }

    public void UpdateParams(int fovDelta, double roff, bool centered, double distance = 5.0)
    {
        _fov = fovDelta; _roff = roff; _centered = centered; _dist = distance;
        Redraw();
    }

    private void Redraw()
    {
        Children.Clear();
        double total = 40 + _fov;
        double half = total / 2 * Math.PI / 180;
        double cx = W / 2;

        // Scale cam-to-player gap from distance (5.0 default = 110px gap, range ~50-180px)
        double gap = Math.Clamp(_dist * 16, 50, 180);
        double charY = 100;
        double camYPos = charY + gap;
        double coneLen = gap + 60;

        double actualRo = _centered ? 0 : 0.5 * (1.0 + (-_roff) / 0.5);
        double off = Math.Clamp(actualRo * 55, -160, 160);
        double camX = cx - off;

        double lx = camX - coneLen * Math.Tan(half);
        double rx = camX + coneLen * Math.Tan(half);
        double ty = camYPos - coneLen;

        var cone = new Polygon
        {
            Points = new PointCollection { new(camX, camYPos), new(lx, ty), new(rx, ty) },
            Fill = ConeBrush, Opacity = 0.6
        };
        Children.Add(cone);

        AddDashedLine(camX, camYPos, lx, ty, ConeLineBrush, 4, 4);
        AddDashedLine(camX, camYPos, rx, ty, ConeLineBrush, 4, 4);

        var charDot = new Ellipse { Width = 16, Height = 16, Fill = CharBrush };
        SetLeft(charDot, cx - 8); SetTop(charDot, charY - 8);
        Children.Add(charDot);
        AddText(cx, charY + 16, "player", LabelBrush, 9);

        var camIcon = new Rectangle
        {
            Width = 14, Height = 10, Fill = CamBrush,
            Stroke = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
            StrokeThickness = 1
        };
        SetLeft(camIcon, camX - 7); SetTop(camIcon, camYPos - 5);
        Children.Add(camIcon);
        AddText(camX, camYPos + 14, "cam", LabelBrush, 9);

        double fovTextY = (charY + 20 + camYPos) / 2;
        AddText(camX, fovTextY, $"{total:F0}\u00b0 FoV", ConeLineBrush, 14,
            fontFamily: "Consolas", fontWeight: FontWeights.Bold);

        AddText(W / 2, 12, "FIELD OF VIEW  (top-down)", DimBrush, 10);

        AddLine(cx, ty + 6, cx, ty - 6, GroundBrush, 1);
        var arrow = new Polygon
        {
            Points = new PointCollection { new(cx - 4, ty - 2), new(cx + 4, ty - 2), new(cx, ty - 10) },
            Fill = GroundBrush
        };
        Children.Add(arrow);
        AddText(cx, ty - 16, "forward", GroundBrush, 9);
    }

    private void AddLine(double x1, double y1, double x2, double y2, Brush stroke, double thickness)
    {
        Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = stroke, StrokeThickness = thickness });
    }

    private void AddDashedLine(double x1, double y1, double x2, double y2, Brush stroke, double dash, double gap)
    {
        Children.Add(new Line
        {
            X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
            Stroke = stroke, StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { dash, gap }
        });
    }

    private void AddText(double x, double y, string text, Brush foreground, double fontSize,
        string fontFamily = "Segoe UI", FontWeight? fontWeight = null)
    {
        var tb = new TextBlock
        {
            Text = text, Foreground = foreground,
            FontSize = fontSize, FontFamily = new FontFamily(fontFamily),
            FontWeight = fontWeight ?? FontWeights.Normal
        };
        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        SetLeft(tb, x - tb.DesiredSize.Width / 2);
        SetTop(tb, y - tb.DesiredSize.Height / 2);
        Children.Add(tb);
    }
}
