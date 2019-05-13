using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Digital.Tools.Data.Models
{
    public class GovUkNotifyPersonalisation
    {
        public GovUkNotifyPersonalisation()
        {
            Personalisation = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Personalisation { get; set; }
    }
}
