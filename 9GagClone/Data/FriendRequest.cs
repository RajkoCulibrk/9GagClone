namespace _9GagClone.Data;

public class FriendRequest
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public User Requester { get; set; }

    public int RequestedId { get; set; }
    public User Requested { get; set; }

    public DateTime RequestDate { get; set; }
    public FriendShipRequestsStatus Status { get; set; } = FriendShipRequestsStatus.Pending;
}