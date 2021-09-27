using Rocket.API;

namespace LobbySpawnpoint
{
    public class Config : IRocketPluginConfiguration
    {
        public void LoadDefaults()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Yaw = 0;
            SetUp = false;
        }

        public float X;
        public float Y;
        public float Z;
        public float Yaw;
        public bool SetUp;
    }
}
