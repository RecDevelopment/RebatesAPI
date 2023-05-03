namespace RebatesAPI.DTO
{
    public class ConfirmationRequest
    {
        public string ItemId { get; set; }
        public int quantity { get; set; }
        public decimal Value { get; set; }
        public string Source { get; set; }
        public string ID { get; set; }
        public decimal Total { get; set; }
        public string Confirmation { get; set; } //Approved or Rejected 


    }
}
