using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tzen.Framework.SQL
{
    public class DefaultCmdBuilder<T> : CmdBuilder<T> where T : class
    {
        public DefaultCmdBuilder() :
            base()
        {

        }
        public DefaultCmdBuilder(CmdTemplate cmdTemplate) : 
            base(cmdTemplate)
        {

        }
    }
}
