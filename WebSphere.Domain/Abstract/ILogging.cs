using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSphere.Domain.Abstract
{
    public interface ILogging
    {
        bool Logged(string logEventLevel, string message, string nameSpace, string currClass);
    }
}
