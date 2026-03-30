using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UltimateCameraMod.Controls;

public class CameraPreview : Canvas
{
    private const double W = 420, H = 370;
    private const double GroundY = 315, CharX = 310, CharH = 220;
    private const double DistScale = 26, HeadR = 14;

    private static readonly Brush BgBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e1e1e"));
    private static readonly Brush GroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a4a4a"));
    private static readonly Brush CharBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c8a24e"));
    private static readonly Brush CamBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aaaaaa"));
    private static readonly Brush LabelBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888"));
    private static readonly Brush MeasureBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
    private static readonly Brush GuideBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));
    private static readonly Brush GuideTextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
    private static readonly Brush TextPrimaryBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0e0e0"));

    private double _dist = 5.0, _up = -0.2;
    private string _label = "Heroic";

    public CameraPreview()
    {
        Width = W; Height = H;
        Background = BgBrush;
        ClipToBounds = true;
        Redraw();
    }

    public void UpdateParams(double dist, double up, string label = "Custom")
    {
        _dist = dist; _up = up; _label = label;
        Redraw();
    }

    private double HeadTop => GroundY - CharH;
    private double HeadCy => HeadTop + HeadR;
    private double Scale => (GroundY - HeadCy) / 1.5;

    private double BodyY(double upOffset) => HeadCy + Math.Abs(upOffset) * Scale;
    private double ShoulderY => BodyY(-0.2);
    private double HipY => BodyY(-0.8);
    private double KneeY => BodyY(-1.2);

    private string HeightZone(double v) => v switch
    {
        >= 0.1 => "above head",
        >= -0.1 => "head",
        >= -0.25 => "shoulder",
        >= -0.4 => "chest",
        >= -0.65 => "stomach",
        >= -0.9 => "hip",
        >= -1.1 => "waist",
        >= -1.35 => "knee",
        >= -1.48 => "shin",
        _ => "ground",
    };

    private void Redraw()
    {
        Children.Clear();

        var guides = new (double Y, string Label, string Value)[]
        {
            (HeadCy, "head", "0.0"),
            (ShoulderY, "shoulder", "-0.2"),
            (HipY, "hip", "-0.8"),
            (KneeY, "knee", "-1.2"),
            (GroundY, "", "-1.5"),
        };
        foreach (var (gy, gl, gv) in guides)
        {
            AddDashedLine(10, gy, W - 10, gy, GuideBrush, 1, 6);
            string rt = !string.IsNullOrEmpty(gv) && !string.IsNullOrEmpty(gl) ? $"{gl}  ({gv})" : gv ?? gl;
            if (!string.IsNullOrEmpty(rt))
                AddText(W - 8, gy, rt, GuideTextBrush, 9, HorizontalAlignment.Right);
        }

        double camX = Math.Max(40, Math.Min(CharX - _dist * DistScale, CharX - 40));
        double camY = Math.Max(32, Math.Min(HeadCy - _up * Scale, GroundY - 12));

        DrawGround();
        DrawCharacter();
        DrawCamera(camX, camY);

        // Sight line from camera to character head
        AddDashedLine(camX + 15, camY, CharX, HeadCy, 
            new SolidColorBrush(Color.FromArgb(0x40, 0xc8, 0xa2, 0x4e)), 4, 4);

        // Distance measurement
        double ly = GroundY + 20;
        AddDashedLine(camX, ly, CharX, ly, MeasureBrush, 3, 3);
        AddLine(camX, ly - 4, camX, ly + 4, MeasureBrush, 1);
        AddLine(CharX, ly - 4, CharX, ly + 4, MeasureBrush, 1);
        AddText((camX + CharX) / 2, ly + 14, $"Distance: {_dist:F1}", LabelBrush, 10);

        AddText(W / 2, 14, _label, TextPrimaryBrush, 13, fontWeight: FontWeights.Bold);

        string zone = HeightZone(_up);
        AddText(camX - 14, camY, $"{zone}  ({_up:+0.0;-0.0})", LabelBrush, 10, HorizontalAlignment.Right);
    }

    private void DrawGround()
    {
        AddLine(10, GroundY, W - 10, GroundY, GroundBrush, 1);
        for (double gx = 18; gx < W - 10; gx += 14)
            AddLine(gx, GroundY + 1, gx - 5, GroundY + 7, GroundBrush, 1);
    }

    private void DrawCharacter()
    {
        double x = CharX, gy = GroundY;
        double hc = HeadCy, sh = ShoulderY, hp = HipY;

        var head = new Ellipse { Width = HeadR * 2, Height = HeadR * 2, Fill = CharBrush };
        SetLeft(head, x - HeadR); SetTop(head, hc - HeadR);
        Children.Add(head);

        double nk = hc + HeadR;
        AddLine(x, nk, x, hp, CharBrush, 3);
        AddLine(x - 22, sh + 20, x, sh, CharBrush, 2.5);
        AddLine(x, sh, x + 22, sh + 20, CharBrush, 2.5);
        AddLine(x, hp, x - 18, gy - 2, CharBrush, 2.5);
        AddLine(x, hp, x + 18, gy - 2, CharBrush, 2.5);
    }

    private void DrawCamera(double cx, double cy)
    {
        double bw = 24, bh = 16;
        var body = new Rectangle
        {
            Width = bw, Height = bh,
            Fill = CamBrush, Stroke = Brushes.LightGray, StrokeThickness = 1
        };
        SetLeft(body, cx - bw / 2); SetTop(body, cy - bh / 2);
        Children.Add(body);

        var lens = new Polygon
        {
            Points = new PointCollection
            {
                new(cx + bw / 2, cy - 6),
                new(cx + bw / 2 + 9, cy - 8),
                new(cx + bw / 2 + 9, cy + 8),
                new(cx + bw / 2, cy + 6),
            },
            Fill = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
            Stroke = new SolidColorBrush(Color.FromRgb(0xBB, 0xBB, 0xBB)),
            StrokeThickness = 1
        };
        Children.Add(lens);

        var viewfinder = new Rectangle
        {
            Width = 10, Height = 7,
            Fill = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
            Stroke = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
            StrokeThickness = 1
        };
        SetLeft(viewfinder, cx - 5); SetTop(viewfinder, cy - bh / 2 - 7);
        Children.Add(viewfinder);
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
        HorizontalAlignment align = HorizontalAlignment.Center, FontWeight? fontWeight = null)
    {
        var tb = new TextBlock
        {
            Text = text, Foreground = foreground,
            FontSize = fontSize, FontFamily = new FontFamily("Segoe UI"),
            FontWeight = fontWeight ?? FontWeights.Normal
        };
        tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double left = align switch
        {
            HorizontalAlignment.Right => x - tb.DesiredSize.Width,
            HorizontalAlignment.Left => x,
            _ => x - tb.DesiredSize.Width / 2
        };
        SetLeft(tb, left); SetTop(tb, y - tb.DesiredSize.Height / 2);
        Children.Add(tb);
    }
}
