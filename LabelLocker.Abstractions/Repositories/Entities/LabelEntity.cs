using System.ComponentModel.DataAnnotations;

namespace LabelLocker.Repositories.Entities;

public class LabelEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public LabelState State { get; set; }
    [Timestamp]
    public byte[] ReservationToken { get; set; } = Array.Empty<byte>();
}