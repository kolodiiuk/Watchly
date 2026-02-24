using Watchly.Domain.Enums;

namespace Watchly.Domain.Entities;

public sealed class Comment
{
    public int ContentId { get; set; }
    
    public ContentType ContentType { get; set; }
}
