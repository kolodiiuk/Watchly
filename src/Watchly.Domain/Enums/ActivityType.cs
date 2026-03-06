namespace Watchly.Domain.Enums;

[Flags]
public enum ActivityType
{
    Watched = 1 << 0,

    Rated = 1 << 1,
}
