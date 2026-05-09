using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPF_Test.Vista.UserControls
{
    /// <summary>
    /// Lógica de interacción para TestingBlock.xaml
    /// </summary>
    public partial class TestingBlock : UserControl
    {
        // Content Text Dependency Property
        public static readonly DependencyProperty ContentTextProperty =
            DependencyProperty.Register(
                nameof(ContentText),
                typeof(string),
                typeof(TestingBlock),
                new PropertyMetadata(string.Empty));

        public string ContentText
        {
            get => (string)GetValue(ContentTextProperty);
            set => SetValue(ContentTextProperty, value);
        }

        // ButtonForeground Dependency Property
        public static readonly DependencyProperty ButtonForegroundProperty =
            DependencyProperty.Register(
                nameof(ButtonForeground),
                typeof(Brush),
                typeof(TestingBlock),
                new PropertyMetadata(Brushes.Black)
                );

        public Brush ButtonForeground
        {
            get => (Brush)GetValue(ButtonForegroundProperty);
            set => SetValue(ButtonForegroundProperty, value);
        }

        // ── HoverBackground ──
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register(
                nameof(HoverBackground), 
                typeof(Brush), 
                typeof(TestingBlock),
                new PropertyMetadata(Brushes.LightGray));

        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        // ── HoverForeground ──
        public static readonly DependencyProperty HoverForegroundProperty =
            DependencyProperty.Register(
                nameof(HoverForeground), 
                typeof(Brush), 
                typeof(TestingBlock),
                new PropertyMetadata(Brushes.Black));

        public Brush HoverForeground
        {
            get => (Brush)GetValue(HoverForegroundProperty);
            set => SetValue(HoverForegroundProperty, value);
        }

        // ── PressedBackground ──
        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register(
                nameof(PressedBackground), 
                typeof(Brush), 
                typeof(TestingBlock),
                new PropertyMetadata(Brushes.DarkGray));

        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        // ── PressedForeground ──
        public static readonly DependencyProperty PressedForegroundProperty =
            DependencyProperty.Register(
                nameof(PressedForeground), 
                typeof(Brush), 
                typeof(TestingBlock),
                new PropertyMetadata(Brushes.White));

        public Brush PressedForeground
        {
            get => (Brush)GetValue(PressedForegroundProperty);
            set => SetValue(PressedForegroundProperty, value);
        }

        // ── Helper: anima el color de un SolidColorBrush ──
        private void AnimateColor(SolidColorBrush brush, Color to, double durationSeconds)
        {
            var animation = new ColorAnimation
            {
                To = to,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut } // suavizado
            };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        private SolidColorBrush GetBorderBrush() =>
            (SolidColorBrush)btn.Template.FindName("borderColor", btn);

        private SolidColorBrush GetTextBrush() =>
            (SolidColorBrush)btn.Template.FindName("textColor", btn);

        // ── Eventos ──
        private void btn_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimateColor(GetBorderBrush(), ((SolidColorBrush)HoverBackground).Color, 0.1);
            AnimateColor(GetTextBrush(), ((SolidColorBrush)HoverForeground).Color, 0.1);
        }

        private void btn_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateColor(GetBorderBrush(), Colors.Transparent, 0.1);
            AnimateColor(GetTextBrush(), ((SolidColorBrush)ButtonForeground).Color, 0.1);
        }

        private void btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AnimateColor(GetBorderBrush(), ((SolidColorBrush)PressedBackground).Color, 0.1);
            AnimateColor(GetTextBrush(), ((SolidColorBrush)PressedForeground).Color, 0.1);
        }

        private void btn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AnimateColor(GetBorderBrush(), ((SolidColorBrush)HoverBackground).Color, 0.15);
            AnimateColor(GetTextBrush(), ((SolidColorBrush)HoverForeground).Color, 0.15);
        }

        public TestingBlock()
        {
            InitializeComponent();
            btn.DataContext = this;

            btn.Loaded += (s, e) =>
            {
                GetTextBrush().Color = ((SolidColorBrush)ButtonForeground).Color;
                GetBorderBrush().Color = Colors.Transparent;
            };
        }
    }
}
