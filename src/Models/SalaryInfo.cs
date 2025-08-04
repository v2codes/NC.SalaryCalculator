
using CommunityToolkit.Mvvm.ComponentModel;

namespace NC.SalaryCalculator.Models
{
    /// <summary>
    /// 薪资配置
    /// </summary>
    public partial class SalaryInfo : ObservableObject
    {
        /// <summary>
        /// 薪资类型
        /// </summary>
        [ObservableProperty]
        private string? _salaryType = "月薪";

        /// <summary>
        /// 薪资金额(¥)
        /// </summary>
        [ObservableProperty]
        private float? _salaryAmount = 0.00f;

        /// <summary>
        /// 每日工作时长(小时)
        /// </summary>
        [ObservableProperty]
        private int? _workHoursPerDay = 8;

        /// <summary>
        /// 每周工作天数
        /// </summary>
        [ObservableProperty]
        private int? _workDaysPerWeek = 5;

        /// <summary>
        /// 上班起始时间
        /// </summary>
        [ObservableProperty]
        private DateTime? _workStartTime = DateTime.Now.Date.AddHours(9);
    }
}
