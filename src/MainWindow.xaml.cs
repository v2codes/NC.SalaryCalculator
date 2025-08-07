using NC.SalaryCalculator.Data;
using NC.SalaryCalculator.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NC.SalaryCalculator;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.DataContext = new MainWindowViewModel();
        InitializeComponent();

        // 绑定标题栏按钮事件
        this.Loaded += MainWindow_Loaded;
        this.Closing += OnClosing;
        
        BindingButtonEvents();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 不包含任务栏的工作区域
        var workingArea = SystemParameters.WorkArea;

        this.Left = workingArea.Right - this.ActualWidth - 10;
        this.Top = workingArea.Bottom - this.ActualHeight - 10;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!ApplicationConfig.AllowExit)
        {
            var result = MessageBox.Show("今天的钱挣够了吗？", "退出", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true; // 取消关闭
            }
        }
    }

    private void BindingButtonEvents()
    {
        btnMinus.Click += (s, e) =>
        {
            //WindowState = WindowState.Minimized;
            this.Hide(); // 隐藏窗口（不退出）
        };
        //btnMaximize.Click += (s, e) =>
        //{
        //    if (WindowState == WindowState.Maximized)
        //    {
        //        gridMainWindow.Margin = new Thickness(0);
        //        WindowState = WindowState.Normal;
        //    }
        //    else
        //    {
        //        gridMainWindow.Margin = new Thickness(5);
        //        WindowState = WindowState.Maximized;
        //    }
        //};
        btnClose.Click += (s, e) =>
        {
            this.Close();
        };

        titleBar.MouseMove += (s, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        };

        //titleBar.MouseDoubleClick += (s, e) =>
        //{
        //    if (WindowState == WindowState.Maximized)
        //    {
        //        gridMainWindow.Margin = new Thickness(0);
        //        WindowState = WindowState.Normal;
        //    }
        //    else
        //    {
        //        gridMainWindow.Margin = new Thickness(5);
        //        WindowState = WindowState.Maximized;
        //    }
        //};
    }

    private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
    }
}