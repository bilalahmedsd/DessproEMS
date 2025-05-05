using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Services
{
    public interface ILoggerService 
    {
        Task LogErrorAsync(Exception ex, [CallerMemberName] string controllerName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    }
}
