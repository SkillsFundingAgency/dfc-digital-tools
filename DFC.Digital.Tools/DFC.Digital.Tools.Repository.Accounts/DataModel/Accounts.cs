using System;
using System.Collections.Generic;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public partial class Accounts
    {
        public string Name { get; set; }

        public string SfaProviderUserType { get; set; }

        public string A1lifecycleState { get; set; }

        public string UPIN { get; set; }

        public DateTime Createtimestamp { get; set; }

        public string Mail { get; set; }

        public DateTime? Modifytimestamp { get; set; }

        public string Uid { get; set; }
    }
}
