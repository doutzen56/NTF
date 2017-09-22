using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTF.MQ
{
    public interface IRabbitProxy
    {
        bool Publish<T>(string queueName, T message);

    }
}
