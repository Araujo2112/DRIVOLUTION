namespace ApiTexPact.DTO
{
    public class CheckpointDTO
    {
        public int CheckpointId { get; set; }
        
        public string CheckpointCode { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public int SectionId { get; set; }
    }
}