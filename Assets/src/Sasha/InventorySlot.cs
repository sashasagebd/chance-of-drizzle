public class InventorySlot
{
    public Item ItemReference { get; private set; }
    public int Count { get; set; }

    public InventorySlot(Item item, int count)
    {
        ItemReference = item;
        Count = count;
    }
}