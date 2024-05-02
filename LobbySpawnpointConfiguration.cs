using Rocket.API;

namespace LobbySpawnpoint;

public class LobbySpawnpointConfiguration : IRocketPluginConfiguration
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Yaw { get; set; }
    public bool SetUp { get; set; }
    public bool PlainTextSaveFile { get; set; }
    public void LoadDefaults()
    {
        X = 0;
        Y = 0;
        Z = 0;
        Yaw = 0;
        SetUp = false;
        PlainTextSaveFile = false;
    }
}