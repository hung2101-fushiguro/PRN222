namespace FPTCanteen.KitchenServer.Dtos;

public class OrderTicketDto
{
    public string PosName { get; set; } = default!;
    public List<TicketLineDto> Lines { get; set; } = [];
}

public class TicketLineDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}
