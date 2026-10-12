namespace Catalog.ResourceParameters
{
    public class ProductResourceParameters
    {
        const int maxPageSize = 20;
        private int _pageSize = 10;
        public string? MainCategory { get; set; }
        public string? SearchQuery { get; set; }
        public int PageNumber { get; set; } = 1;
        public string OrderBy { get; set; } = "Name";
        public string? Fields { get; set; }
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Min(maxPageSize, value);
        }
    }
}
