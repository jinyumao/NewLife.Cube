﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NewLife.Web;
using XCode;
using XCode.Membership;
using XCode.Statistics;
using NewLife.Cube.Charts;
using static XCode.Membership.VisitStat;

namespace NewLife.Cube.Admin.Controllers
{
    /// <summary>访问统计控制器</summary>
    [DisplayName("访问统计")]
    [Description("每个页面每天的访问统计信息")]
    [Area("Admin")]
    public class VisitStatController : ReadOnlyEntityController<VisitStat>
    {
        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<VisitStat> Search(Pager p)
        {
            var model = new VisitStatModel();
            model.Fill(p.Params, StatLevels.Day);
            model.Page = p["p"];

            p.RetrieveState = true;

            var list = VisitStat.Search(model, p["dtStart"].ToDateTime(), p["dtEnd"].ToDateTime(), p);

            if (list.Count > 0)
            {
                var chart = new ECharts();
                chart.SetX(list, _.Time, e => e.Time.ToString("yyyy-MM-dd"));
                chart.SetY(_.Times);
                chart.AddLine(list, _.Times, null, true);
                chart.Add(list, _.Users);
                chart.Add(list, _.IPs);
                chart.Add(list, _.Error);
                chart.SetTooltip();

                var chart2 = new ECharts();
                chart2.AddPie(list, _.Times, e => new { name = e.Time.ToString("yyyy-MM-dd"), value = e.Times });

                ViewBag.Charts = new[] { chart };
                ViewBag.Charts2 = new[] { chart2 };
            }

            return list;
        }

        /// <summary>菜单不可见</summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        protected override IDictionary<MethodInfo, Int32> ScanActionMenu(IMenu menu)
        {
            if (menu.Visible)
            {
                menu.Visible = false;
                (menu as IEntity).Update();
            }

            return base.ScanActionMenu(menu);
        }
    }
}