using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HWallpaper.Model
{
    public class TypeList
    {
        public string id { get; set; }
        public string name { get; set; }
        public int order_num { get; set; }
        /// <summary>
        /// 2020-02-11 15:38:04
        /// </summary>
        public string create_time { get; set; }

        /// <summary>
        /// 标签（_全部_ _category_性感女神_  _category_美女模特_）
        /// </summary>
        public string tag { get; set; }

    }
}
