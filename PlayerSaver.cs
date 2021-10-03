using System;
using System.IO;
using System.Linq;
namespace LobbySpawnpoint
{
    public class PlayerSaver
    {
        public const string FILE_LOCATION = @"Plugins\LobbySpawnpoint\online_players.dat";
        public ulong[] PrevOnline;
        public void Read()
        {
            byte[] file = File.ReadAllBytes(FILE_LOCATION);
            if (file.Length % sizeof(ulong) != 0)
            {
                Rocket.Core.Logging.Logger.LogWarning($"Detected corrupted player save file, size " +
                    $"{file.Length} bytes should be divisible by {sizeof(ulong)} bytes.");
            }
            int amt = file.Length;
            PrevOnline = new ulong[amt];
            for (int i = 0; i < amt; i++)
            {
                int start = i * sizeof(ulong);
                PrevOnline[i] = BitConverter.ToUInt64(file, start);
            }
        }
        public void TryAdd(ulong id)
        {
            if (PrevOnline.Contains(id)) return;
            using (FileStream stream = File.Open(FILE_LOCATION, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                stream.Seek(0, SeekOrigin.End);
                stream.Write(BitConverter.GetBytes(id), 0, sizeof(ulong));
                if (PrevOnline == null)
                    PrevOnline = new ulong[1] { id };
                else
                {
                    ulong[] old = PrevOnline;
                    PrevOnline = new ulong[PrevOnline.Length + 1];
                    Buffer.BlockCopy(old, 0, PrevOnline, 0, old.Length);
                    PrevOnline[PrevOnline.Length - 1] = id;
                }
            }
        }
        public bool HasJoined(ulong id)
        {
            if (!File.Exists(FILE_LOCATION) || PrevOnline == null)
                Read();
            return PrevOnline.Contains(id);
        }
    }
}
