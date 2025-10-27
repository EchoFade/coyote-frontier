namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// Enum for different types of roleplay incentives
/// to help determine which components should to be check for
/// when calculating the final modifier.
/// </summary>
public enum RpiActionType : byte
{
    None,
    Mining,
    Salvage,
    Cooking,
    Bartending,
    Medical,
    Janitorial,
    Engineering,
    Atmos,
    Pilot,
    Librarian,
    Chaplain,
    Horny,
    StationPrincess,
    Combat,
}
