
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using NC.SalaryCalculator.Models;
using NC.SalaryCalculator.Utils;
using NC.SalaryCalculator.View;
using System.Text.Json;
using System.Windows;

namespace NC.SalaryCalculator.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        #region 基础配置
        /// <summary>
        /// 薪资类型
        /// </summary>
        public string _storageKey = "Salary";

        /// <summary>
        /// 薪资类型
        /// </summary>
        public List<string> SalaryTypeOptions { get; } = new() { "时薪", "日薪", "月薪", "年薪" };

        /// <summary>
        /// 薪资配置
        /// </summary>
        [ObservableProperty]
        private SalaryInfo _salaryInfo = new();

        /// <summary>
        /// 主页可见性
        /// </summary>
        [ObservableProperty]
        private Visibility _homeVisibility = Visibility.Visible;

        /// <summary>
        /// 设置可见性
        /// </summary>
        [ObservableProperty]
        private Visibility _settingVisibility = Visibility.Collapsed;

        #endregion

        #region 薪资计算
        /// <summary>
        /// 计算薪资收入任务
        /// </summary>
        private Task _calculateIncomeTask;

        /// <summary>
        /// 计算任务取消令牌源
        /// </summary>
        private CancellationTokenSource CancellationTokenSource;

        /// <summary>
        /// 手动重置事件，用于控制计算任务的暂停和继续
        /// </summary>
        private ManualResetEvent _manualEvent = new ManualResetEvent(true);

        private string _startContent = "开　　始";
        private string _pauseContent = "暂　　停";
        private string _continueContent = "继　　续";
        private float _hourlySalary = 0.00f; // 每小时收入
        private int _totalPausedTime = 0; // 累计暂停时间
        private DateTime? _lastPausedTime; // 上一次暂停时间

        /// <summary>
        /// 开始按钮内容
        /// </summary>
        [ObservableProperty]
        private string _btnStartContent;

        /// <summary>
        /// 今日收入
        /// </summary>
        [ObservableProperty]
        private float? _todayTotalIncome = 0.00f;

        /// <summary>
        /// 今日目标收入
        /// </summary>
        [ObservableProperty]
        private float? _todayTargetIncome = 0.00f;

        /// <summary>
        /// 每秒收入
        /// </summary>
        [ObservableProperty]
        private float? _salaryPerSecond = 0.00f;

        /// <summary>
        /// 每分钟收入
        /// </summary>
        [ObservableProperty]
        private float? _salaryPerMinute = 0.00f;

        /// <summary>
        /// 每小时收入
        /// </summary>
        [ObservableProperty]
        private float? _salaryPerHour = 0.00f;

        /// <summary>
        /// 今日目标达成率
        /// </summary>
        [ObservableProperty]
        private float? _targetAchievementRate = 0.00f;

        /// <summary>
        /// 收入里程碑消息
        /// </summary>
        [ObservableProperty]
        private string _incomeMilestoneMessage = string.Empty;
        #endregion

        /// <summary>
        /// 弹窗消息
        /// </summary>
        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        /// <summary>
        /// 数据存储服务
        /// </summary>
        private DataStorageService _dataStorageService;

        public MainWindowViewModel()
        {
            SnackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            _dataStorageService = DataStorageService.Instance;

            // 初始化
            BtnStartContent = _startContent;
            CancellationTokenSource = new CancellationTokenSource();

            // 加载本地缓存
            var salaryInfo = _dataStorageService.GetValue<SalaryInfo>(_storageKey, null!, true);
            if (salaryInfo != null)
            {
                if (salaryInfo.WorkStartTime.HasValue)
                {
                    salaryInfo.WorkStartTime = DateTime.Now.Date.AddHours(salaryInfo.WorkStartTime.Value.Hour)
                                                                .AddMinutes(salaryInfo.WorkStartTime.Value.Minute)
                                                                .AddSeconds(salaryInfo.WorkStartTime.Value.Second);
                }
                SalaryInfo = salaryInfo;
            }

            // TODO 通过请求工作日列表

            // 启动定时弹窗线程
            _ = Task.Run(async () =>
            {
                await Task.Delay(10 * 1000);
                await SweetPopupAsync();
            });
        }


        #region TODO API获取工作日列表

        #endregion

        #region 基础配置
        /// <summary>
        /// 切换设置界面
        /// </summary>
        [RelayCommand]
        private void OpenSetting()
        {
            // 切换界面
            if (HomeVisibility == Visibility.Visible)
            {
                HomeVisibility = Visibility.Collapsed;
                SettingVisibility = Visibility.Visible;
            }
            else
            {
                HomeVisibility = Visibility.Visible;
                SettingVisibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        [RelayCommand]
        private async Task SaveSettings()
        {
            if (!ValidationSalaryInfo())
            {
                return;
            }

            // 保存本地缓存
            var jsonText = JsonSerializer.Serialize(SalaryInfo);
            _dataStorageService.SetValue(_storageKey, jsonText, true);

            // 切换内容
            HomeVisibility = Visibility.Visible;
            SettingVisibility = Visibility.Collapsed;

            if (_calculateIncomeTask != null && !_calculateIncomeTask.IsCompleted)
            {
                CancellationTokenSource.Cancel();
                while (true)
                {
                    if (_calculateIncomeTask == null || _calculateIncomeTask.IsCompleted)
                    {
                        break;
                    }
                    await Task.Delay(100);
                }
            }
        }

        /// <summary>
        /// 取消设置
        /// </summary>
        [RelayCommand]
        private void CancelSetting()
        {
            var salaryInfo = _dataStorageService.GetValue<SalaryInfo>(_storageKey, null!, true);
            SalaryInfo = salaryInfo;

            // 切换内容
            HomeVisibility = Visibility.Visible;
            SettingVisibility = Visibility.Collapsed;
        }
        #endregion

        #region 薪资计算
        /// <summary>
        /// 计算薪资收入
        /// </summary>
        [RelayCommand]
        private async Task TriggerCalculation()
        {
            // 开始计算
            if (BtnStartContent == _startContent)
            {
                await StartCalculateAsync();
            }
            // 暂停计算
            else if (BtnStartContent == _pauseContent)
            {
                BtnStartContent = _continueContent;
                _lastPausedTime = DateTime.Now;
                _manualEvent.Reset();
            }
            // 继续计算
            else
            {
                BtnStartContent = _pauseContent;
                // 计算时间差
                var diff = DateTime.Now - _lastPausedTime!.Value;
                var seconds = diff.TotalSeconds;
                _totalPausedTime += (int)seconds;
                _lastPausedTime = null;
                _manualEvent.Set();
            }
        }

        /// <summary>
        /// 开始计算
        /// </summary>
        private async Task StartCalculateAsync()
        {
            if (!ValidationSalaryInfo())
            {
                return;
            }

            // 按钮内容重置
            BtnStartContent = _pauseContent;

            // 清除已计算结果
            ClearCalculateResult();

            // 计算时薪、分薪、秒薪
            _hourlySalary = CalculateHourlySalary();
            SalaryPerHour = (float)Math.Round(_hourlySalary, 2, MidpointRounding.AwayFromZero);
            SalaryPerMinute = (float)Math.Round(_hourlySalary / 60, 2, MidpointRounding.AwayFromZero);
            SalaryPerSecond = (float)Math.Round(_hourlySalary / 3600, 2, MidpointRounding.AwayFromZero);
            TodayTargetIncome = (float)Math.Round(_hourlySalary * SalaryInfo.WorkHoursPerDay!.Value, 2, MidpointRounding.AwayFromZero);

            // 计算薪资收入
            if (_calculateIncomeTask == null || _calculateIncomeTask.IsCompleted)
            {
                CancellationTokenSource = new CancellationTokenSource();
                _calculateIncomeTask = Task.Run(CalculateAsync, CancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// 计算时薪
        /// </summary>
        private float CalculateHourlySalary()
        {
            float? hourlySalary = 0.00f;

            switch (SalaryInfo.SalaryType)
            {
                case "年薪":
                    // 年薪 ÷ (每周工作天数 × 52周 × 每天工作小时)
                    hourlySalary = SalaryInfo.SalaryAmount / (SalaryInfo.WorkDaysPerWeek * 52 * SalaryInfo.WorkHoursPerDay);
                    break;
                case "月薪":
                    // 月薪 ÷ (每周工作天数 × 4.33周 × 每天工作小时)
                    hourlySalary = SalaryInfo.SalaryAmount / (SalaryInfo.WorkDaysPerWeek * 4.33f * SalaryInfo.WorkHoursPerDay);
                    break;
                case "日薪":
                    // 日薪 ÷ 每天工作小时
                    hourlySalary = SalaryInfo.SalaryAmount / SalaryInfo.WorkHoursPerDay;
                    break;
                case "时薪":
                    // 已经是时薪
                    hourlySalary = SalaryInfo.SalaryAmount;
                    break;
            }

            return hourlySalary ?? 0.00f;
        }

        /// <summary>
        /// 计算收入
        /// </summary>
        /// <returns></returns>
        private async Task CalculateAsync()
        {
            while (true)
            {
                if (CancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                // 如果暂停了，等待手动继续
                //if (_manualEvent.WaitOne(0) == false)
                //{
                //    _lastPauseTime = DateTime.Now;
                //}
                _manualEvent.WaitOne();

                var dtNow = DateTime.Now;
                if (dtNow < SalaryInfo.WorkStartTime)
                {
                    await Task.Delay(1000);
                    continue;
                }

                // 计算时间差
                var diff = dtNow - SalaryInfo.WorkStartTime!.Value;
                var seconds = diff.TotalSeconds;

                // 今日总收入，排除暂停时间
                var salaryAmount = (seconds - _totalPausedTime) * (_hourlySalary / 3600);
                TodayTotalIncome = (float)Math.Round(salaryAmount, 2, MidpointRounding.AwayFromZero);

                // 今日进度
                var targetAchievementRate = (float)Math.Round(TodayTotalIncome.Value / TodayTargetIncome!.Value * 100, 2, MidpointRounding.AwayFromZero);
                TargetAchievementRate = targetAchievementRate;

                var message = GetIncomeMilestoneMessage(TodayTotalIncome.Value);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    IncomeMilestoneMessage = message;
                }

                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// 获取收入里程碑彩蛋
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private string GetIncomeMilestoneMessage(float amount)
        {
            foreach (var m in IncomeMilestone.IncomeMilestoneList)
            {
                if (amount > m.Amount)
                {
                    return m.Message;
                }
                //if (!m.Triggered && amount >= m.Amount)
                //{
                //    m.Triggered = true;
                //    return m.Message;
                //}
            }
            return string.Empty;
        }

        /// <summary>
        /// 重置计算结果
        /// </summary>
        [RelayCommand]
        private async Task ResetCalculation()
        {
            CancellationTokenSource.Cancel();

            while (true)
            {
                if (_calculateIncomeTask == null || _calculateIncomeTask.IsCompleted)
                {
                    break;
                }
                await Task.Delay(100);
            }

            // 按钮内容重置
            BtnStartContent = _startContent;
            ClearCalculateResult();
        }

        private void ClearCalculateResult()
        {
            // 重置计算结果
            _hourlySalary = 0.00f;
            _lastPausedTime = null;
            _totalPausedTime = 0;

            TodayTotalIncome = 0.00f;
            TodayTargetIncome = 0.00f;
            SalaryPerHour = 0.00f;
            SalaryPerMinute = 0.00f;
            SalaryPerSecond = 0.00f;
            TargetAchievementRate = 0.00f;
            IncomeMilestoneMessage = string.Empty;
        }

        /// <summary>
        /// 校验设置项
        /// </summary>
        /// <returns></returns>
        private bool ValidationSalaryInfo()
        {
            if (string.IsNullOrWhiteSpace(SalaryInfo.SalaryType) || !SalaryTypeOptions.Contains(SalaryInfo.SalaryType))
            {
                SnackbarMessageQueue.Enqueue("请选择薪资类型！");
                return false;
            }

            if (SalaryInfo.SalaryAmount <= 0)
            {
                SnackbarMessageQueue.Enqueue("请设置薪资金额！");
                return false;
            }

            if (SalaryInfo.WorkHoursPerDay <= 0 || SalaryInfo.WorkHoursPerDay > 24)
            {
                SnackbarMessageQueue.Enqueue("请设置每日工作时长！");
                return false;
            }

            if (SalaryInfo.WorkDaysPerWeek <= 0 || SalaryInfo.WorkDaysPerWeek > 7)
            {
                SnackbarMessageQueue.Enqueue("请设置每周工作天数！");
                return false;
            }

            if (SalaryInfo.WorkStartTime == null)
            {
                SnackbarMessageQueue.Enqueue("请设置上班起始时间！");
                return false;
            }
            return true;
        }
        #endregion

        #region 定时弹窗线程
        private DateTime? _lastPopupTime = null;

        /// <summary>
        /// 定时弹窗提醒线程
        /// </summary>
        /// <returns></returns>
        private async Task SweetPopupAsync()
        {
            while (true)
            {
                var dtNow = DateTime.Now;

                // 如果上次弹出在同一小时内，则跳过
                if (_lastPopupTime.HasValue
                    && _lastPopupTime.Value.Hour == dtNow.Hour
                    && _lastPopupTime.Value.Date == dtNow.Date)
                {
                    await Task.Delay(30 * 1000); // 每30秒检查一次
                    continue;
                }

                _lastPopupTime = dtNow;
                var message = GetSweetMessage(dtNow);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var helloWindow = new HelloWindow();
                    helloWindow.SetMessage(message);
                    helloWindow.Show();
                });

                await Task.Delay(30 * 1000); // 每30秒检查一次
            }
        }

        /// <summary>
        /// 整点提示语
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        private string GetSweetMessage(DateTime now)
        {
            switch (now.Hour)
            {
                case 9: return "早安我的球球～新的一天开始啦，愿今天也是元气满满的你！☀️🌸";
                case 12: return "球球该吃午饭啦～不许偷懒不吃饭哦！乖乖去吃饭才有力气变美美~ 🍱💕";
                case 13: return "球球要乖乖午休哦~ 休息一下才有精神继续发光发热 🐷💤";
                case 14: return "起床啦懒猪~ 午觉时间结束！一起打起精神继续加油吧 🐱🌼";
                case 17: return "叮～ 球球快下班~ 收拾收拾准备回家咯~ 今天也辛苦啦！🏡❤️";
                default: return "叮咚～ 又是一个小时过去啦，球球记得活动一下，别久坐哦～ 💕";
            }
        }
        #endregion
    }
}