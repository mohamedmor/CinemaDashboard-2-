namespace CinemaDashboard.Models.Cart
{
    // Lives only in Session (JSON-serialized), not in the database.
    public class CartItem
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; }
        public string MainImg { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
