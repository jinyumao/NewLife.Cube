﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using NewLife.Collections;
using NewLife.Data;
using NewLife.Security;
using NewLife.Serialization;
using XCode;
using XCode.Configuration;

namespace NewLife.Cube.Charts
{
    /// <summary>ECharts实例</summary>
    public class ECharts : IExtend3
    {
        #region 属性
        /// <summary>名称</summary>
        public String Name { get; set; } = Rand.NextString(8);

        /// <summary>宽度。单位px，负数表示百分比，默认-100</summary>
        public Int32 Width { get; set; } = -100;

        /// <summary>高度。单位px，负数表示百分比，默认300px</summary>
        public Int32 Height { get; set; } = 300;

        /// <summary>CSS样式</summary>
        public String Style { get; set; }

        /// <summary>CSS类</summary>
        public String Class { get; set; }

        /// <summary>标题。字符串或匿名对象</summary>
        public ChartTitle Title { get; set; }

        /// <summary>提示</summary>
        public Object Tooltip { get; set; } = new Object();

        /// <summary>提示</summary>
        public Object Legend { get; set; }

        /// <summary>X轴</summary>
        public Object XAxis { get; set; }

        /// <summary>Y轴</summary>
        public Object YAxis { get; set; }

        /// <summary>系列数据</summary>
        public IList<Series> Series { get; set; }

        /// <summary>扩展字典</summary>
        [ScriptIgnore]
        public IDictionary<String, Object> Items { get; set; } = new NullableDictionary<String, Object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>扩展数据</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Object this[String key] { get => Items[key]; set => Items[key] = value; }
        #endregion

        #region 方法
        /// <summary>添加系列数据</summary>
        /// <param name="series"></param>
        public void Add(Series series)
        {
            if (Series == null) Series = new List<Series>();

            Series.Add(series);
        }

        /// <summary>添加系列数据</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">实体列表</param>
        /// <param name="field">要使用数据的字段</param>
        /// <param name="type">图表类型，默认折线图line</param>
        /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
        /// <returns></returns>
        public Series Add<T>(IList<T> list, FieldItem field, String type = "line", Func<T, Object> selector = null) where T : IEntity
        {
            if (type.IsNullOrEmpty()) type = "line";

            var sr = new Series
            {
                Name = field?.DisplayName ?? field.Name,
                Type = type,
                Data = list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray(),
            };

            Add(sr);
            return sr;
        }

        /// <summary>添加曲线系列数据</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">实体列表</param>
        /// <param name="field">要使用数据的字段</param>
        /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
        /// <param name="smooth">折线光滑</param>
        /// <returns></returns>
        public Series AddLine<T>(IList<T> list, FieldItem field, Func<T, Object> selector = null, Boolean smooth = false) where T : IEntity
        {
            var sr = new Series
            {
                Name = field?.DisplayName ?? field.Name,
                Type = "line",
                Data = list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray(),
                Smooth = smooth,
            };

            Add(sr);
            return sr;
        }

        /// <summary>添加曲线系列数据</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">实体列表</param>
        /// <param name="field">要使用数据的字段</param>
        /// <param name="selector">数据选择器，默认null时直接使用字段数据</param>
        /// <returns></returns>
        public Series AddPie<T>(IList<T> list, FieldItem field, Func<T, Object> selector = null) where T : IEntity
        {
            var sr = new Series
            {
                Name = field?.DisplayName ?? field.Name,
                Type = "pie",
                Data = list.Select(e => selector == null ? e[field.Name] : selector(e)).ToArray(),
            };

            Add(sr);
            return sr;
        }

        /// <summary>设置X轴</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="field"></param>
        /// <param name="selector"></param>
        public void SetX<T>(IList<T> list, FieldItem field, Func<T, String> selector = null) where T : IEntity
        {
            XAxis = new
            {
                data = list.Select(e => selector == null ? e[field.Name] + "" : selector(e)).ToArray()
            };
        }

        /// <summary>设置Y轴</summary>
        /// <param name="name"></param>
        /// <param name="type">
        /// 坐标轴类型。
        /// value 数值轴，适用于连续数据。
        /// category 类目轴，适用于离散的类目数据，为该类型时必须通过 data 设置类目数据。
        /// time 时间轴，适用于连续的时序数据，与数值轴相比时间轴带有时间的格式化，在刻度计算上也有所不同，例如会根据跨度的范围来决定使用月，星期，日还是小时范围的刻度。
        /// log 对数轴。适用于对数数据。
        /// </param>
        public void SetY(String name, String type = "value")
        {
            YAxis = new { name = name, type = type };
        }

        /// <summary>设置工具栏</summary>
        /// <param name="trigger">
        /// 触发类型。
        /// item, 数据项图形触发，主要在散点图，饼图等无类目轴的图表中使用。
        /// axis, 坐标轴触发，主要在柱状图，折线图等会使用类目轴的图表中使用。
        /// none, 什么都不触发。
        /// </param>
        /// <param name="axisPointerType">坐标轴指示器配置项。cross，坐标系会自动选择显示哪个轴的 axisPointer</param>
        /// <param name="backgroundColor"></param>
        public void SetTooltip(String trigger = "axis", String axisPointerType = "cross", String backgroundColor = "#6a7985")
        {
            Tooltip = new
            {
                trigger = trigger,
                axisPointer = new
                {
                    type = axisPointerType,
                    label = new
                    {
                        backgroundColor = backgroundColor
                    }
                },
            };
        }

        /// <summary>设置提示</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="field"></param>
        /// <param name="selector"></param>
        public void SetLegend<T>(IList<T> list, FieldItem field, Func<T, String> selector = null) where T : IEntity
        {
            Legend = list.Select(e => selector == null ? e[field.Name] + "" : selector(e)).ToArray();
        }

        /// <summary>构建选项Json</summary>
        /// <returns></returns>
        public String Build()
        {
            var dic = new Dictionary<String, Object>();

            // 标题
            var title = Title;
            if (title != null) dic[nameof(title)] = title;

            // 提示
            var tooltip = Tooltip;
            if (tooltip != null) dic[nameof(tooltip)] = tooltip;

            // 提示
            var legend = Legend;
            if (legend == null) legend = Series.Select(e => e.Name).ToArray();
            if (legend != null)
            {
                if (legend is String str)
                    legend = new { data = new[] { str } };
                else if (legend is String[] ss)
                    legend = new { data = ss };

                dic[nameof(legend)] = legend;
            }

            // X轴
            var xAxis = XAxis;
            if (xAxis != null)
            {
                if (xAxis is String str)
                    xAxis = new { data = new[] { str } };
                else if (xAxis is String[] ss)
                    xAxis = new { data = ss };

                dic[nameof(xAxis)] = xAxis;
            }

            // Y轴
            var yAxis = YAxis;
            if (yAxis != null) dic[nameof(yAxis)] = yAxis;

            // 系列数据
            var series = Series;
            if (series != null) dic[nameof(series)] = series;

            return dic.ToJson(false, false, true);
        }
        #endregion
    }
}