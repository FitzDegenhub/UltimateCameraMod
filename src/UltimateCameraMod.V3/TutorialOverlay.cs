using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UltimateCameraMod.V3;

public class TutorialOverlay : Canvas
{
    private readonly List<TutorialStep> _steps;
    private int _currentStep;
    private readonly Action _onComplete;
    private readonly Window _owner;

    public sealed record TutorialStep(
        string Title,
        string Description,
        Func<FrameworkElement?> TargetElement);

    public TutorialOverlay(List<TutorialStep> steps, Action onComplete, Window owner)
    {
        _steps = steps;
        _onComplete = onComplete;
        _owner = owner;
        Background = Brushes.Transparent;
        IsHitTestVisible = true;
        Visibility = Visibility.Visible;

        // Recalculate on resize
        _owner.SizeChanged += (_, _) => { if (_currentStep < _steps.Count) ShowStep(); };
    }

    public void Start()
    {
        _currentStep = 0;
        ShowStep();
    }

    private void ShowStep()
    {
        Children.Clear();

        if (_currentStep >= _steps.Count)
        {
            Complete();
            return;
        }

        var step = _steps[_currentStep];
        var target = step.TargetElement();

        double w = _owner.ActualWidth;
        double h = _owner.ActualHeight;
        if (w < 1) w = 1400;
        if (h < 1) h = 900;

        Width = w;
        Height = h;

        Rect spotlightRect;
        Point cardPosition;

        if (target != null && target.IsVisible)
        {
            try
            {
                // Get position relative to the window's content area
                var transform = target.TransformToVisual(_owner);
                var topLeft = transform.Transform(new Point(0, 0));
                spotlightRect = new Rect(
                    topLeft.X - 10, topLeft.Y - 10,
                    target.ActualWidth + 20, target.ActualHeight + 20);

                // Clamp spotlight inside window
                if (spotlightRect.Right > w) spotlightRect.Width = w - spotlightRect.X - 4;
                if (spotlightRect.Bottom > h) spotlightRect.Height = h - spotlightRect.Y - 4;

                // Position card: prefer below, then above, then to the right
                if (spotlightRect.Bottom + 210 < h)
                {
                    // Below spotlight
                    cardPosition = new Point(
                        Math.Clamp(spotlightRect.X, 8, w - 390),
                        spotlightRect.Bottom + 16);
                }
                else if (spotlightRect.Y - 210 > 0)
                {
                    // Above spotlight
                    cardPosition = new Point(
                        Math.Clamp(spotlightRect.X, 8, w - 390),
                        spotlightRect.Y - 210);
                }
                else
                {
                    // To the right, vertically centered
                    cardPosition = new Point(
                        Math.Clamp(spotlightRect.Right + 16, 8, w - 390),
                        Math.Clamp(spotlightRect.Y + spotlightRect.Height / 2 - 100, 8, h - 220));
                }
            }
            catch
            {
                spotlightRect = new Rect(w / 2 - 150, h / 2 - 50, 300, 100);
                cardPosition = new Point(w / 2 - 170, h / 2 + 70);
            }
        }
        else
        {
            spotlightRect = new Rect(w / 2 - 150, h / 2 - 50, 300, 100);
            cardPosition = new Point(w / 2 - 170, h / 2 + 70);
        }

        // Dark mask with spotlight cutout
        var fullRect = new RectangleGeometry(new Rect(0, 0, w, h));
        var cutout = new RectangleGeometry(spotlightRect, 8, 8);
        var combined = new CombinedGeometry(GeometryCombineMode.Exclude, fullRect, cutout);

        var mask = new System.Windows.Shapes.Path
        {
            Data = combined,
            Fill = new SolidColorBrush(Color.FromArgb(0xCC, 0, 0, 0)),
            IsHitTestVisible = true
        };
        Children.Add(mask);

        // Spotlight border (gold accent)
        var spotlight = new Rectangle
        {
            Width = spotlightRect.Width,
            Height = spotlightRect.Height,
            Stroke = new SolidColorBrush(Color.FromRgb(0xC8, 0xA2, 0x4E)),
            StrokeThickness = 2,
            RadiusX = 8, RadiusY = 8,
            Fill = Brushes.Transparent,
            IsHitTestVisible = false
        };
        SetLeft(spotlight, spotlightRect.X);
        SetTop(spotlight, spotlightRect.Y);
        Children.Add(spotlight);

        // Info card
        var card = BuildInfoCard(step.Title, step.Description, _currentStep, _steps.Count);
        SetLeft(card, cardPosition.X);
        SetTop(card, cardPosition.Y);
        Children.Add(card);
    }

    private Border BuildInfoCard(string title, string description, int stepIndex, int totalSteps)
    {
        var stepCounter = new TextBlock
        {
            Text = $"Step {stepIndex + 1} of {totalSteps}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 16, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xA2, 0x4E)),
            Margin = new Thickness(0, 0, 0, 8)
        };

        var descBlock = new TextBlock
        {
            Text = description,
            FontSize = 12, TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            LineHeight = 18,
            Margin = new Thickness(0, 0, 0, 14)
        };

        var nextBtn = new Button
        {
            Content = stepIndex < totalSteps - 1 ? "Next" : "Get Started",
            Width = stepIndex < totalSteps - 1 ? 80 : 110,
            Height = 32, FontSize = 12, FontWeight = FontWeights.SemiBold,
            Background = new SolidColorBrush(Color.FromRgb(0xC8, 0xA2, 0x4E)),
            Foreground = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A)),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand, Padding = new Thickness(14, 4, 14, 4)
        };
        nextBtn.Click += (_, _) => { _currentStep++; ShowStep(); };

        var skipBtn = new Button
        {
            Content = "Skip tutorial",
            Width = 90, Height = 32, FontSize = 11,
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand, Margin = new Thickness(8, 0, 0, 0)
        };
        skipBtn.Click += (_, _) => Complete();

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };
        buttonPanel.Children.Add(nextBtn);
        buttonPanel.Children.Add(skipBtn);

        var stack = new StackPanel();
        stack.Children.Add(stepCounter);
        stack.Children.Add(titleBlock);
        stack.Children.Add(descBlock);
        stack.Children.Add(buttonPanel);

        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x24, 0x24, 0x24)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xA2, 0x4E)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(22, 18, 22, 18),
            Width = 370,
            Child = stack,
            IsHitTestVisible = true
        };
    }

    private void Complete()
    {
        Children.Clear();
        Visibility = Visibility.Collapsed;
        _onComplete?.Invoke();
    }
}
