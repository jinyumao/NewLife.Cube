﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using NewLife.Web;
using XCode.Membership;

namespace NewLife.Cube.Admin.Controllers
{
    /// <summary>部门</summary>
    [DataPermission(null, "ManagerID={#userId}")]
    [DisplayName("部门")]
    [Area("Admin")]
    public class DepartmentController : EntityController<Department>
    {
        static DepartmentController()
        {
            MenuOrder = 95;

            ListFields.RemoveField("Ex1");
            ListFields.RemoveField("Ex2");
            ListFields.RemoveField("Ex3");
            ListFields.RemoveField("Ex4");
            ListFields.RemoveField("Ex5");
            ListFields.RemoveField("Ex6");

            ListFields.RemoveUpdateField();
            ListFields.RemoveCreateField();
        }

        /// <summary>搜索数据集</summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected override IEnumerable<Department> Search(Pager p)
        {
            var id = p["id"].ToInt(-1);
            if (id > 0)
            {
                var list = new List<Department>();
                var entity = Department.FindByID(id);
                if (entity != null) list.Add(entity);
                return list;
            }

            var parentId = p["parentId"].ToInt(-1);
            var enable = p["enable"]?.ToBoolean();
            var visible = p["visible"]?.ToBoolean();

            return Department.Search(parentId, enable, visible, p["Q"], p);
        }
    }
}