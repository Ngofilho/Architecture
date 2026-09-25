using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class LogisticsMessage : Message
    {
        public OrderMessage Order { get; set; }
    }
}
