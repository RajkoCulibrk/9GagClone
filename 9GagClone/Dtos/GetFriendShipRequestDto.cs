using _9GagClone.Data;

namespace _9GagClone.Dtos;

public class GetFriendShipRequestDto
{
    public int Id { get; set; }
    public FriendDto Requester { get; set; }
    public FriendDto Requested { get; set; }

    public DateTime RequestDate { get; set; }
    public FriendShipRequestsStatus Status { get; set; }
}