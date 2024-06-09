namespace HelixAPI.Models
{
    public class QueryDto(string SortByValue)
    {
        public List<FilterDto> Filters { get; set; } = new List<FilterDto>();
        public int Size { get; set; } = 100;
        public int Offset { get; set; } = 0;
        public string SortBy { get; set; } = SortByValue; // Default sort by Id
        public string SortOrder { get; set; } = "asc";
        public string? Fields { get; set; } = null;
    }

    public class FilterDto
    {
        public string Property { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }
    }
}
