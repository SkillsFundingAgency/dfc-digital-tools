using System;
using System.Collections.Generic;

namespace DFC.Digital.Tools.Repository.Accounts
{
    public partial class Audit
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }

        public string Notes { get; set; }

        public byte[] TimeStamp { get; set; }
    }
}
