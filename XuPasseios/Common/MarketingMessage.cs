using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class MarketingMessage : Message
    {
        public CampaignDTO campaign { get; set; }
        public OrderMessage Order { get; set; }
    }

    public class CampaignDTO
    {
        public int idCampaign { get; set; }
        public string Campaign { get; set; }
        public DateTime CampaignStart { get; set; }
        public DateTime CampaignEnd { get; set; }
        public int CampaignAuthor { get; set; }
    }
}
