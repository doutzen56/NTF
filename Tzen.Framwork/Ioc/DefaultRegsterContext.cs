﻿using System.Reflection;

namespace Tzen.Framework.Ioc
{

    public class DefaultRegsterContext
    {
        public Assembly Assembly { get; private set; }
        public IIocManager IocManager { get; private set; }
        public DefaultRegsterContext(Assembly assembly,IIocManager iocManager)
        {
            this.Assembly = assembly;
            this.IocManager = iocManager;
        }
    }
}