namespace Portal.Models.Admin
{
    public class LookUp
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<LookUpList> LookUpLists { get; set; }
    }
}