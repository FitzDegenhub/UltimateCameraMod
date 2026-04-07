using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace UltimateCameraMod.Avalonia.Controls;

public class FovPreview : Canvas
{
    private const double W = 420, H = 370;

    private static readonly IBrush BgBrush = new SolidColorBrush(Color.Parse("#1e1e1e"));
    private static readonly IBrush CharBrush = new SolidColorBrush(Color.Parse("#c8a24e"));
    private static readonly IBrush CamBrush = new SolidColorBrush(Color.Parse("#aaaaaa"));
    private static readonly IBrush ConeBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x5a, 0x48, 0x20));
    private static readonly IBrush ConeLineBrush = new SolidColorBrush(Color.Parse("#7a6830"));
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#888888"));
    private static readonly IBrush GroundBrush = new SolidColorBrush(Color.Parse("#4a4a4a"));
    private static readonly IBrush DimBrush = new SolidColorBrush(Color.Parse("#777777"));
    private static readonly IBrush CamStrokeBrush = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));

    private static readonly IBrush MeasureBrush = new SolidColorBrush(Color.Parse("#666666"));
    private static readonly IDashStyle ConeDash = new DashStyle(new double[] { 4, 4 }, 0);
    private static readonly IDashStyle MeasureDash = new DashStyle(new double[] { 3, 3 }, 0);

    private static readonly FontFamily SegoeUI = new("Segoe UI, Inter, Liberation Sans, DejaVu Sans, sans-serif");
    private static readonly FontFamily Consolas = new("Consolas, Liberation Mono, DejaVu Sans Mono, monospace");

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

        // Scale cam-to-player gap from distance (5.0 default = 140px gap, range ~70-240px)
        double gap = Math.Clamp(_dist * 22, 70, 240);
        double charY = 120;
        double camYPos = charY + gap;
        double coneLen = gap + 80;

        double actualRo = _centered ? 0 : 0.5 * (1.0 + (-_roff) / 0.5);
        double off = Math.Clamp(actualRo * 70, -180, 180);
        double camX = cx + off;

        double lx = camX - coneLen * Math.Tan(half);
        double rx = camX + coneLen * Math.Tan(half);
        double ty = camYPos - coneLen;

        var cone = new Polygon
        {
            Points = new global::Avalonia.Collections.AvaloniaList<Point> { new(camX, camYPos), new(lx, ty), new(rx, ty) },
            Fill = ConeBrush, Opacity = 0.6
        };
        Children.Add(cone);

        AddDashedLine(camX, camYPos, lx, ty, ConeLineBrush, ConeDash);
        AddDashedLine(camX, camYPos, rx, ty, ConeLineBrush, ConeDash);

        var charDot = new Ellipse { Width = 16, Height = 16, Fill = CharBrush };
        SetLeft(charDot, cx - 8); SetTop(charDot, charY - 8);
        Children.Add(charDot);
        AddText(cx, charY + 16, "player", LabelBrush, 9);

        var camIcon = new Rectangle
        {
            Width = 14, Height = 10, Fill = CamBrush,
            Stroke = CamStrokeBrush,
            StrokeThickness = 1
        };
        SetLeft(camIcon, camX - 7); SetTop(camIcon, camYPos - 5);
        Children.Add(camIcon);
        AddText(camX, camYPos + 14, "cam", LabelBrush, 9);

        double fovTextY = (charY + 20 + camYPos) / 2;
        AddText(camX, fovTextY, $"{total:F0}\u00b0 FoV", ConeLineBrush, 14,
            fontFamily: Consolas, fontWeight: FontWeight.Bold);

        // Distance measurement between player and camera
        double distLineX = Math.Min(cx, camX) - 30;
        AddDashedLine(distLineX, charY, distLineX, camYPos, MeasureBrush, MeasureDash);
        AddLine(distLineX - 3, charY, distLineX + 3, charY, MeasureBrush, 1);
        AddLine(distLineX - 3, camYPos, distLineX + 3, camYPos, MeasureBrush, 1);
        AddText(distLineX - 8, (charY + camYPos) / 2, $"{_dist:F1}", LabelBrush, 9,
            fontFamily: Consolas);

        AddText(W / 2, 12, "FIELD OF VIEW  (top-down)", DimBrush, 10);

        AddLine(cx, ty + 6, cx, ty - 6, GroundBrush, 1);
        var arrow = new Polygon
        {
            Points = new global::Avalonia.Collections.AvaloniaList<Point> { new(cx - 4, ty - 2), new(cx + 4, ty - 2), new(cx, ty - 10) },
            Fill = GroundBrush
        };
        Children.Add(arrow);
        AddText(cx, ty - 16, "forward", GroundBrush, 9);
    }

    private void AddLine(double x1, double y1, double x2, double y2, IBrush stroke, double thickness)
    {
        Children.Add(new Line { StartPoint = new Point(x1, y1), EndPoint = new Point(x2, y2), Stroke = stroke, StrokeThickness = thickness });
    }

    private void AddDashedLine(double x1, double y1, double x2, double y2, IBrush stroke, IDashStyle dashStyle)
    {
        Children.Add(new Line
        {
            StartPoint = new Point(x1, y1), EndPoint = new Point(x2, y2),
            Stroke = stroke, StrokeThickness = 1,
            StrokeDashArray = new global::Avalonia.Collections.AvaloniaList<double>(dashStyle.Dashes ?? Array.Empty<double>())
        });
    }

    private void AddText(double x, double y, string text, IBrush foreground, double fontSize,
        FontFamily? fontFamily = null, FontWeight? fontWeight = null)
    {
        var tb = new TextBlock
        {
            Text = text, Foreground = foreground,
            FontSize = fontSize, FontFamily = fontFamily ?? SegoeUI,
            FontWeight = fontWeight ?? FontWeight.Normal
        };
        tb.Measure(Size.Infinity);
        SetLeft(tb, x - tb.DesiredSize.Width / 2);
        SetTop(tb, y - tb.DesiredSize.Height / 2);
        Children.Add(tb);
    }
}
