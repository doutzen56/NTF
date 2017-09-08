using System;

namespace NTF.Logger
{
    /// <summary>
    /// 默认日志实现
    /// </summary>
    /// <remarks>
    /// 底层默认提供一个空的日志实现，调用层通过注入对应日志实现类来写日志
    /// </remarks>
    internal class NullLoger : INtfLog
    {
        public void Error(Exception ex)
        {
            
        }

        public void Error(string msg)
        {
            
        }

        public void Fatal(Exception ex)
        {
            
        }

        public void Fatal(string msg)
        {
            
        }

        public void Info(Exception ex)
        {
            
        }

        public void Info(string msg)
        {
            
        }

        public void Trace(Exception ex)
        {
            
        }

        public void Trace(string msg)
        {
            
        }

        public void Warn(string msg)
        {
            
        }

        public void Warn(Exception ex)
        {
            
        }
    }
}
