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
            using (FileStream stream = File.Open(FILE_LOCATION, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                long amt = stream.Length / sizeof(ulong);
                PrevOnline = new ulong[amt];
                byte[] buffer = new byte[sizeof(ulong)];
                for (int i = 0; i < amt; i++)
                {
                    int start = i * sizeof(ulong);
                    stream.Read(buffer, start, sizeof(ulong));
                    PrevOnline[i] = BitConverter.ToUInt64(buffer, start);
                }
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
