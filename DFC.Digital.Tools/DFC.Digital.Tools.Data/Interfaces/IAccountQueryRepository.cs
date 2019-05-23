using DFC.Digital.Tools.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Digital.Tools.Data.Interfaces
{
    public interface IAccountQueryRepository
    {
        IQueryable<Account> GetAccountsThatStillNeedProcessing(DateTime cutOffDate);
    }
}
