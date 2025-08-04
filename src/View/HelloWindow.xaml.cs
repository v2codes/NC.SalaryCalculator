
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace NC.SalaryCalculator.View
{
    /// <summary>
    /// HelloWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HelloWindow : Window
    {
        private const double ShowDurationSeconds = 3.0;

        public HelloWindow()
        {
            InitializeComponent();
            this.Loaded += HelloWindow_Loaded;
        }

        private async void HelloWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var workingArea = SystemParameters.WorkArea;

            this.Left = workingArea.Right + this.ActualWidth + 10;
            //this.Left = workingArea.Right - this.ActualWidth - 10; // 目标位置
            this.Top = workingArea.Top + 20;

            // 动画：向左滑入
            var slideIn = new DoubleAnimation
            {
                From = this.Left,
                To = SystemParameters.WorkArea.Right - this.Width - 10,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(Window.LeftProperty, slideIn);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var workingArea = SystemParameters.WorkArea;
            
            // 动画：向右滑出
            var slideOut = new DoubleAnimation
            {
                From = this.Left,
                To = workingArea.Right + this.ActualWidth + 10,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            slideOut.Completed += (s, _) => this.Close();
            this.BeginAnimation(Window.LeftProperty, slideOut);
        }

        public void SetMessage(string message)
        {
            tbMessage.Text = message;
        }
    }
}
