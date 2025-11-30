
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RakipBul.Models
{public class DeviceToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public string Topic { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
}
