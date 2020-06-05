﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using NewLife.Collections;
using NewLife.Data;

namespace NewLife.Cube.Charts
{
    /// <summary>系列。一组数值以及他们映射成的图</summary>
    public class Series : IExtend3
    {
        #region 属性
        /// <summary>图表类型</summary>
        public String Type { get; set; }
        //public SeriesTypes Type { get; set; }

        /// <summary>名称</summary>
        public String Name { get; set; }

        /// <summary>数据</summary>
        public Object Data { get; set; }

        /// <summary>折线光滑</summary>
        public Boolean Smooth { get; set; }

        /// <summary>扩展字典</summary>
        [ScriptIgnore]
        public IDictionary<String, Object> Items { get; set; } = new NullableDictionary<String, Object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>扩展数据</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Object this[String key] { get => Items[key]; set => Items[key] = value; }
        #endregion
    }
}