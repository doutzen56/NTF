using IntegrationAdmin;
using NTF.Ioc;
using NTF.Provider;
using NTF.Uow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class Test : ITransient
    {
        [UnitOfWork]
        public virtual void Test1()
        {
            var ioc = IocManager.Instance;
            var up = ioc.Resolve<IDbContext<u8_Update_Jackpot>>();
            var model = up.FirstOrDefault();
            up.Update(model);
        }
    }
}
