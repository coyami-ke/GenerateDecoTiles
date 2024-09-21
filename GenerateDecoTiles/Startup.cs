using UnityModManagerNet;

namespace GenerateDecoTiles
{
    internal static class Startup
    {
        internal static void Load(UnityModManager.ModEntry modEntry)
        {
            MainClass.Setup(modEntry);
        }
    }
}
