using Common;

namespace Marketing.Dtos
{
    public class SaleDTO : OrderMessage
    {
        public CampaignDTO campaign{ get; set; }
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
