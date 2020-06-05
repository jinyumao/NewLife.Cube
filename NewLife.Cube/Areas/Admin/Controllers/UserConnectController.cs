﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube.Entity;
using NewLife.Web;
using XCode;
using XCode.Membership;

namespace NewLife.Cube.Admin.Controllers
{
    /// <summary>用户链接控制器</summary>
    [DataPermission(null, "UserID={#userId}")]
    [DisplayName("用户链接")]
    [Description("第三方登录信息")]
    [Area("Admin")]
    public class UserConnectController : EntityController<UserConnect>
    {
        static UserConnectController()
        {
            ListFields.RemoveField("AccessToken");
            ListFields.RemoveField("RefreshToken");
            ListFields.RemoveField("Avatar");
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

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<UserConnect> Search(Pager p)
        {
            var key = p["Q"];
            var userid = p["userid"].ToInt();
            var provider = p["provider"];
            var start = p["dtStart"].ToDateTime();
            var end = p["dtEnd"].ToDateTime();

            return UserConnect.Search(provider, userid, start, end, key, p);
        }
    }
}