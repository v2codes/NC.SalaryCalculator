
namespace NC.SalaryCalculator.Models
{
    /// <summary>
    /// 达成目标彩蛋提示
    /// </summary>
    public class IncomeMilestone
    {
        /// <summary>
        /// 达成的金额
        /// </summary>
        public float Amount { get; set; }

        /// <summary>
        /// 提示语
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 是否已触发，避免重复提醒
        /// </summary>
        public bool Triggered { get; set; } = false;

        /// <summary>
        /// 收入里程碑列表
        /// </summary>
        public static List<IncomeMilestone> IncomeMilestoneList = new List<IncomeMilestone>
        {
            new IncomeMilestone { Amount = 1314.00f, Message = "一生一世，真·上班成就解锁！💍" },
            new IncomeMilestone { Amount = 1000.00f, Message = "四位数了，开始向富人进发！🚀" },
            new IncomeMilestone { Amount = 666.00f, Message = "神秘代码666达成，打工皇帝降临！👑" },
            new IncomeMilestone { Amount = 520.00f, Message = "你对自己是真爱啊！❤️" },
            new IncomeMilestone { Amount = 314.00f, Message = "π元目标达成，圆满收官！🥧" },
            new IncomeMilestone { Amount = 233.00f, Message = "233收入达成，笑出声～ 哈哈哈" },
            new IncomeMilestone { Amount = 100.00f, Message = "百元大关达成！土豪请加我好友。" },
            new IncomeMilestone { Amount = 88.88f, Message = "发发发发发，钱包要撑不住了！🐷💸" },
            new IncomeMilestone { Amount = 52.00f, Message = "52块钱，可以表白了！💌" },
            new IncomeMilestone { Amount = 20.00f, Message = "可以点杯奶茶犒劳下自己了！🍱" },
            new IncomeMilestone { Amount = 12.34f, Message = "1234，步步高升！📈" },
            new IncomeMilestone { Amount = 10.00f, Message = "你已经赚了一杯奶茶钱！🧋" },
            new IncomeMilestone { Amount = 8.88f, Message = "恭喜发财，时薪到手！💰" },
            new IncomeMilestone { Amount = 6.66f, Message = "6块6，顺顺利利！✨" },
        };
    }
}
