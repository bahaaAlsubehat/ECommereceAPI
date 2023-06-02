namespace Ref.Lect4.DTO
{
    public class OrderDetailsForAdmin
    {
        public string OrderDate { get; set; }
        public string DelivaryDate { get; set; }
        public string TotalPrics { get; set; }
        public string OrderStatus { get; set; }
        public string Note { get; set; }
        public string IsApproved { get; internal set; }

        public List<OrderItemsCart> ItemsinOrder { get; set; }
       public  UserInformationList UserInfo { get; set; }
    }
}
