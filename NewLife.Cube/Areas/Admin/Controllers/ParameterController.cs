﻿using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using XCode.Membership;

namespace NewLife.Cube.Admin.Controllers
{
    /// <summary>字典参数</summary>
    [DisplayName("字典参数")]
    [Area("Admin")]
    public class ParameterController : EntityController<Parameter>
    {
        static ParameterController()
        {
            MenuOrder = 65;

            ListFields.RemoveField("Ex1");
            ListFields.RemoveField("Ex2");
            ListFields.RemoveField("Ex3");
            ListFields.RemoveField("Ex4");
            ListFields.RemoveField("Ex5");
            ListFields.RemoveField("Ex6");
        }
    }
}