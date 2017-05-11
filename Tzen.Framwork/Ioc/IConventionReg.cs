﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tzen.Framwork.Ioc
{
    /// <summary>
    /// 约定注册接口定义
    /// </summary>
    public interface IConventionRegister
    {
        /// <summary>
        /// 注册程序集中所有类型（遵守约定规则的）到Ioc容器
        /// </summary>
        /// <param name="context"></param>
        void RegisiterAssembly(ConventionRegsterContext context);
    }
}
