using NC.SalaryCalculator.Data;
using System.Configuration;
using System.Data;
using System.Windows;

namespace NC.SalaryCalculator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Mutex? _mutex;
    private static string _appName = "Passion.SmartWarehouse";

    protected override void OnStartup(StartupEventArgs e)
    {
        bool createdNew;
        _mutex = new Mutex(true, _appName, out createdNew);
        if (!createdNew)
        {
            // 已有实例在运行，退出当前程序
            MessageBox.Show("程序已在运行中。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

            // 设置全局配置，允许退出
            ApplicationConfig.AllowExit = true;
            Shutdown();
            return;
        }
    }
}

