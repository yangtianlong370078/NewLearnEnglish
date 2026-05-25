using System.Text;

namespace LearnEnglish.Models
{
    /// <summary>
    /// 分页视图模型
    /// </summary>
    /// <typeparam name="T">分页数据模型</typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// 获取或设置当前页码
        /// </summary>
        /// <value>当前页码</value>
        public int PageIndex { get; private set; }

        /// <summary>
        /// 获取或设置每页显示的数据总条数
        /// </summary>
        /// <value>每页显示的数据总条数</value>
        public int PageSize { get; private set; }

        /// <summary>
        /// 获取当前页数据总条数
        /// </summary>
        /// <value>当前页数据总条数</value>
        public int ItemCount { get; private set; }

        /// <summary>
        /// 获取数据总条数
        /// </summary>
        /// <value>数据总条数</value>
        public int RecordCount { get; private set; }

        /// <summary>
        /// 获取分页总数
        /// </summary>
        /// <value>分页总数</value>
        public int PageCount { get; private set; }

        /// <summary>
        /// 获取是否可以进行上一页跳转
        /// </summary>
        private bool CanPrevious
        {
            get
            {
                return this.PageIndex != 1;
            }
        }

        /// <summary>
        /// 获取是否可以进行下一页跳转
        /// </summary>
        private bool CanNext
        {
            get
            {
                return this.PageIndex < this.PageCount;
            }
        }

        /// <summary>
        /// 获取分页按钮起始页码
        /// </summary>
        private int FirstPageIndex { get; set; }

        /// <summary>
        /// 获取分页按钮结束页码
        /// </summary>
        private int LastPageIndex { get; set; }

        /// <summary>
        /// 翻页js函数名
        /// </summary>
        private string PageChangedJavascriptionFunctionName { get; set; }

        /// <summary>
        /// 是否显示统计信息
        /// </summary>
        private bool ShowCount { get; set; }

        /// <summary>
        /// 初始化一个分页数据视图模型
        /// </summary>
        /// <param name="currentPageItems">当前页需要显示的所有数据列表</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示的数据总条数</param>
        /// <param name="recordCount">需要分页的数据总数</param>
        /// <param name="pageChangedJsFunctionName">页码变更的js函数</param>
        public PagedList(IEnumerable<T> currentPageItems, int pageIndex, int pageSize, int recordCount, string pageChangedJavascriptionFunctionName, bool showCount = false)
        {
            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.RecordCount = recordCount;
            this.PageChangedJavascriptionFunctionName = pageChangedJavascriptionFunctionName;
            if (currentPageItems == null)
            {
                currentPageItems = new List<T>();
            }
            var items = currentPageItems as IList<T> ?? currentPageItems.ToList();
            this.ItemCount = items.Count();
            this.PageCount = (int)Math.Ceiling(recordCount / (double)PageSize);
            this.ShowCount = showCount;

            this.FirstPageIndex = (this.PageIndex <= 3 / 2 + 1 ? 1 : (this.PageIndex - 3 / 2));
            this.LastPageIndex = (this.FirstPageIndex + 3 - 1 >= this.PageCount ? this.PageCount : this.FirstPageIndex + 3 - 1);
            //if (this.LastPageIndex >= this.PageCount)
            //    this.FirstPageIndex = this.LastPageIndex - 7 + 1;
            this.AddRange(items);
        }

        public string BuildHtml()
        {
            if (this.PageCount > 1)
            {
                //<li class=\"disabled\"><a href=\"javascript:;\">上一页</a></li><li class=\"active\"><a href=\"javascript:;\">01</a></li><li><a href=\"javascript:;\" curpage=\"2\">02</a></li><li><a href=\"javascript:;\" curpage=\"3\">03</a></li><li><a href=\"javascript:;\" curpage=\"4\">04</a></li><li><a href=\"javascript:;\" curpage=\"5\">05</a></li><li><a href=\"javascript:;\" curpage=\"6\">06</a></li><li><a href=\"javascript:;\" curpage=\"7\">07</a></li><li><a href=\"javascript:;\" curpage=\"8\">08</a></li><li><a href=\"javascript:;\" curpage=\"9\">09</a></li><li><a href=\"javascript:;\" curpage=\"10\">10</a></li><li><a href=\"javascript:;\" curpage=\"2\">下一页</a></li><input class=\"txtCurPage\" style=\"display:none;\" maxlength=\"6\" name=\"CurPage\" value=\"1\" type=\"text\"></ul></div>
                StringBuilder html = new StringBuilder();

               

                html.AppendLine("<div class=\"dataTables_paginate paging_simple_numbers\" id=\"example_paginate\">");
                html.AppendLine("<ul class=\"pagination\" style=\"margin-bottom: 0.5rem; display: flex; justify-content: center; \">");
                if (this.CanPrevious)
                {
                    html.AppendLine("<li style=\"margin:0px 5px;\"  class=\"\" >");
                    html.AppendLine(string.Format("<div  class=\"ysyse newstyel\"  onclick=\"{0}(1);\"><i style=\"cursor: pointer;position: relative;z-index: 1;\" class=\"icon-Angle-double-left fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>", this.PageChangedJavascriptionFunctionName));
                    html.AppendLine("</li>");
                    html.AppendLine("<li style=\"margin:0px 5px;\"  class=\"\">");
                    html.AppendLine(string.Format("<div  class=\"ysyse newstyel\"   onclick=\"{0}({1});\"><i style=\"cursor: pointer;position: relative;z-index: 1;\" class=\"icon-Angle-left fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>", this.PageChangedJavascriptionFunctionName, this.PageIndex - 1));
                    html.AppendLine("</li>");
                }
                else
                {
                    //html.AppendLine("<li style=\"margin:0px 5px; \"  class=\" disabled\">");
                    //html.AppendLine("<div  class=\"ysyse nrsty example\"   class=\"disabled\"><i style=\"cursor: not-allowed;position: relative;z-index: 1;\" class=\"icon-Angle-double-left fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>");
                    //html.AppendLine("</li>");
                    //html.AppendLine("<li style=\"margin:0px 5px; \"  class=\" disabled\">");
                    //html.AppendLine("<div  class=\"ysyse nrsty example\"   class=\"disabled\"><i style=\"cursor: not-allowed;position: relative;z-index: 1;\"  class=\"icon-Angle-left fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>");
                    //html.AppendLine("</li>");
                }

                for (int i = this.FirstPageIndex; i <= this.LastPageIndex; i++)
                {
                    if (i == this.PageIndex)
                    {
                        html.AppendLine(string.Format("<li style=\"margin:0px 5px;\"  class=\"  active \"><div class=\"ysyse newstyel bg-primary fs-16\" ><span style=\"position: relative;z-index: 1;\">{0}</span></div></li>", i));
                    }
                    else
                    {
                        //  html.AppendLine(string.Format("<li style=\"margin:0px 5px;\"   class=\" ysyse\" ><div class=\"ysyse fs-16\"   onclick=\"{0}({1});\">{1}</div></li>", this.PageChangedJavascriptionFunctionName, i));
                    }
                }

                if (this.CanNext)
                {
                    html.AppendLine("<li style=\"margin:0px 5px;\"   class=\"\">");
                    html.AppendLine(string.Format("<div   class=\"ysyse newstyel\" style=\"\"   onclick=\"{0}({1});\"><i style=\"cursor: pointer;position: relative;z-index: 1;\" class=\"icon-Angle-right fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>", this.PageChangedJavascriptionFunctionName, this.PageIndex + 1));
                    html.AppendLine("</li>");
                    html.AppendLine("<li  style=\"margin:0px 5px;\"  class=\"\">");
                    html.AppendLine(string.Format("<div   class=\"ysyse newstyel\" style=\"\"   onclick=\"{0}({1});\"><i style=\"cursor: pointer;position: relative;z-index: 1;\" class=\"icon-Angle-double-right fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>", this.PageChangedJavascriptionFunctionName, this.PageCount));//因为先前的尾页只是获取当前页码的最后一页，所以现在改成直接获取总页数
                    html.AppendLine("</li>");
                }
                else
                {
                    //html.AppendLine("<li style=\"margin:0px 5px;\"  class=\"paginate_button page-item disabled\">");
                    //html.AppendLine("<div  class=\"ysyse btn\"   class=\"disabled\"><i style=\"cursor: not-allowed;\"  class=\"icon-Angle-right fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>");
                    //html.AppendLine("</li>");
                    //html.AppendLine("<li style=\"margin:0px 5px; \"  class=\"paginate_button page-item disabled\">");
                    //html.AppendLine("<div class=\"ysyse btn\"   class=\"disabled\"><i style=\"cursor: not-allowed;\"  class=\"icon-Angle-double-right fs-24  p-10\"><span class=\"path1\"></span><span class=\"path2\"></span></i></div>");
                    //html.AppendLine("</li>");
                }

                html.AppendLine("</ul>");
                html.AppendLine("</div>");
                //if (this.ShowCount)
                //{
                //    html.AppendLine("<div style=\"width: 100%; text-align: center; \" class=\"dataTables_info\" id=\"example_info\" role=\"status\" aria-live=\"polite\" >");
                //    html.AppendLine(string.Format("每页{0}条，分{2}页； 共<b>{1}</b>条", this.PageSize, this.RecordCount, this.PageCount));
                //    html.AppendLine("</div>");
                //}
                return html.ToString();
            }
            return string.Empty;
        }
    }
}
