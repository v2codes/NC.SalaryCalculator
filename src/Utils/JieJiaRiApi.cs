using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NC.SalaryCalculator.Utils
{
    /// <summary>
    /// 🇨🇳 节假日 API
    /// https://www.jiejiariapi.com/
    /// </summary>
    public class JieJiaRiApi
    {
        /// <summary>
        /// 获取节假日
        /// https://api.jiejiariapi.com/v1/holidays/2025
        /// </summary>
        private static string _holidaysUrl = "https://api.jiejiariapi.com/v1/holidays/{0}";
        /// <summary>
        /// 周六周日接口
        /// https://api.jiejiariapi.com/v1/weekends/2025
        /// </summary>
        private static string _weekendsUrl = "https://api.jiejiariapi.com/v1/weekends/{0}";
        /// <summary>
        /// 工作日接口
        /// https://api.jiejiariapi.com/v1/workdays/2025
        /// </summary>
        private static string _workdaysUrl = "https://api.jiejiariapi.com/v1/workdays/{0}";
        /// <summary>
        /// 是否为节假日接口
        /// https://api.jiejiariapi.com/v1/is_holiday?date=2025-08-03
        /// </summary>
        private static string _isHolidayUrl = "https://api.jiejiariapi.com/v1/is_holiday?date={0}";

        // 单例 HttpClient
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// 获取指定年份的节假日列表
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static async Task<List<DateInfo>> GetHolidayListAsync(int year)
        {
            var url = string.Format(_holidaysUrl, year);
            var result = await GetDateListAsync(url);
            return result;
        }

        /// <summary>
        /// 获取指定年份的周六周日列表
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static async Task<List<DateInfo>> GetWeekendListAsync(int year)
        {
            var url = string.Format(_weekendsUrl, year);
            var result = await GetDateListAsync(url);
            return result;
        }

        /// <summary>
        /// 获取指定年份的工作日列表
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static async Task<List<DateInfo>> GetWorkdayListAsync(int year)
        {
            var url = string.Format(_workdaysUrl, year);
            var result = await GetDateListAsync(url);
            return result;
        }

        /// <summary>
        /// 检查是否为节假日
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static async Task<IsHolidayResult> CheckIsHolidayAsync(DateTime date)
        {
            var url = string.Format(_isHolidayUrl, date.ToString("yyyy-MM-dd"));
            var jsonResult = await _httpClient.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<IsHolidayResult>(jsonResult, _jsonSerializerOptions);
            return result;
        }

        private static async Task<List<DateInfo>> GetDateListAsync(string url)
        {
            var jsonResult = await _httpClient.GetStringAsync(url);
            var dict = JsonSerializer.Deserialize<Dictionary<string, DateInfo>>(jsonResult, _jsonSerializerOptions);
            var result = new List<DateInfo>();
            foreach (var item in dict)
            {
                item.Value.Month = item.Value.Date.Month;
                result.Add(item.Value);
            }

            return result;
        }
    }

    /// <summary>
    /// 日期信息
    /// </summary>
    public class DateInfo
    {
        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 名称
        /// 非国假显示：周一、周二....
        /// 国假日显示：元旦、春节....
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 是否为国假
        /// </summary>
        public bool IsOffDay { get; set; }
    }

    /// <summary>
    /// 是否为国假检查结果
    /// </summary>
    public class IsHolidayResult
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 是否为国假
        /// </summary>
        [JsonPropertyName("is_holiday")]
        public bool IsHoliday { get; set; }
    }
}
