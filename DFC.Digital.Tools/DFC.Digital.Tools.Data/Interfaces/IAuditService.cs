using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IAuditService
    {
        Task AuditAsync(string outputMessage, string inputMessage = null);

        IEnumerable<string> GetAuditRecords();
    }
}
