using System;
using System.Collections.Generic;
using System.Text;

namespace LearnEnglish.Application.Dtos.Statistics
{
    public class StudyStatisticsDto
    {
        /// <summary>
        /// 未熟练
        /// </summary>
        public int UnskilledCount { get; set; }

        /// <summary>
        /// 已掌握
        /// </summary>
        public int MasteredCount { get; set; }

        /// <summary>
        /// 强化中
        /// </summary>
        public int ReinforcementCount { get; set; }

        /// <summary>
        /// 今天学习
        /// </summary>
        public int TodayCount { get; set; }

        /// <summary>
        // /// 日均对比增长率（相较于上次统计的增长百分比，0 表示无增长，正数表示增长，负数表示减少）
        /// </summary>
        public double GrowthRate { get; set; }
    }
}
