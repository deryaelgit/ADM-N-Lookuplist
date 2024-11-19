namespace Portal.Models.Admin
{

    public class LookUpList
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int LookUpId { get; set; }
        public string Name { get; set; }
        public string? ShortName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Icon { get; set; }
        public string? Attributes { get; set; }

        public LookUp LookUp { get; set; }
    }
}