using OpenConfiguration;
using Vintagestory.API.Common;

namespace SpawnersAPI;

public static partial class Configuration
{
    /// <summary>Per-mod logger for OpenConfiguration's own diagnostics (missing/invalid keys, IO errors).</summary>
    internal static ModLogger Logger(ICoreAPI api) => new(api.Logger, "SpawnersAPI");
}
