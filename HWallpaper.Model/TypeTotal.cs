using System;
using System.Collections.Generic;

namespace HWallpaper.Model
{
    public class TypeTotal
    {
        public int consume { get; set; }
        public int total { get; set; }
        public string errmsg { get; set; }
        public int errno { get; set; }

        public List<TypeList> data { get; set; }

    }
}
