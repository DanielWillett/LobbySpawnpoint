using SDG.Unturned;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Logger = Rocket.Core.Logging.Logger;

namespace LobbySpawnpoint;

public class PlayerSaver
{
    private readonly LobbySpawnpoint _plugin;
    private readonly Func<PlayerSaver, bool> _isPlainTextSaveFileGetter;
    private readonly string _dir;
    private readonly byte[] _addBuffer = new byte[sizeof(ulong)];
    private readonly byte[] _newLineAsAscii = Encoding.ASCII.GetBytes(Environment.NewLine);
    private readonly object _sync = new object();
    private bool _saveIsPlainText;
    public ArraySegment<ulong> PreviouslyJoinedPlayers { get; private set; }
    public string FileLocation { get; private set; }
    public bool CurrentFileIsPlainText => _saveIsPlainText;
    public PlayerSaver(LobbySpawnpoint plugin, string dir) : this(plugin, s => s._plugin.Configuration.Instance.PlainTextSaveFile, dir) { }
    public PlayerSaver(LobbySpawnpoint plugin, Func<PlayerSaver, bool> isPlainTextSaveFileGetter, string dir)
    {
        _plugin = plugin;
        _dir = dir;
        _isPlainTextSaveFileGetter = isPlainTextSaveFileGetter;

        UpdateFileLocation();
    }
    private void UpdateFileLocation()
    {
        FileLocation = Path.Combine(_dir, "online_players.dat");
    }
    public void Reset()
    {
        lock (_sync)
        {
            PreviouslyJoinedPlayers = default;
            File.Delete(FileLocation);
            _saveIsPlainText = _isPlainTextSaveFileGetter(this);
        }
    }
    public void Read()
    {
        UpdateFileLocation();

        lock (_sync)
        {
            if (!File.Exists(FileLocation))
            {
                PreviouslyJoinedPlayers = default;
                return;
            }

            bool isPlainText;

            using (FileStream stream = new FileStream(FileLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int firstByte = stream.ReadByte();
                if (firstByte == -1)
                {
                    PreviouslyJoinedPlayers = default;
                    return;
                }

                // all possible steam universes, plain text will start with (int)'7'
                isPlainText = firstByte is not (>= 0 and <= 5);

                if (isPlainText)
                {
                    PreviouslyJoinedPlayers = ReadPlainText((byte)firstByte, stream);
                }
                else
                {
                    PreviouslyJoinedPlayers = ReadBinary((byte)firstByte, stream);
                }
            }

            _saveIsPlainText = isPlainText;
            if (isPlainText == _isPlainTextSaveFileGetter(this))
                return;

            if (isPlainText)
                ConvertToBinary();
            else
                ConvertToPlainText();
        }
    }

    private void ConvertToBinary()
    {
        if (PreviouslyJoinedPlayers.Array == null || PreviouslyJoinedPlayers.Count == 0)
        {
            Reset();
            return;
        }

        using FileStream stream = new FileStream(FileLocation, FileMode.Create, FileAccess.Write, FileShare.Read);

        byte[] addBuffer = ThreadUtil.gameThread == Thread.CurrentThread ? _addBuffer : new byte[8];
        for (int i = 0; i < PreviouslyJoinedPlayers.Count; ++i)
        {
            ulong steam64 = PreviouslyJoinedPlayers.Array[i + PreviouslyJoinedPlayers.Offset];

            WriteSteam64(steam64, addBuffer);

            stream.Write(addBuffer, 0, addBuffer.Length);
        }

        _saveIsPlainText = false;
    }

    private void ConvertToPlainText()
    {
        if (PreviouslyJoinedPlayers.Array == null || PreviouslyJoinedPlayers.Count == 0)
        {
            Reset();
            return;
        }

        using FileStream stream = new FileStream(FileLocation, FileMode.Create, FileAccess.Write, FileShare.Read);

        for (int i = 0; i < PreviouslyJoinedPlayers.Count; ++i)
        {
            ulong steam64 = PreviouslyJoinedPlayers.Array[i + PreviouslyJoinedPlayers.Offset];

            byte[] asciiData = Encoding.ASCII.GetBytes(steam64.ToString("D17", CultureInfo.InvariantCulture));

            if (i != 0)
            {
                stream.Write(_newLineAsAscii, 0, _newLineAsAscii.Length);
            }

            stream.Write(asciiData, 0, asciiData.Length);
        }

        _saveIsPlainText = true;
    }
    private unsafe ArraySegment<ulong> ReadPlainText(byte firstByte, FileStream stream)
    {
        long fileLength = stream.Length;
        Encoding enc = Encoding.ASCII;

        string newLine = Environment.NewLine;
        int newLineSize = enc.GetByteCount(newLine);
        byte* newLineAsAscii = stackalloc byte[newLineSize];
        fixed (char* ptr = newLine)
            enc.GetBytes(ptr, newLine.Length, newLineAsAscii, newLineSize);

        const int steamIdLength = 17;

        int rowSize = newLineSize + steamIdLength;

        long playerCount = (fileLength + newLineSize) / rowSize;
        if (playerCount > int.MaxValue)
        {
            _plugin.LogWarning($"Too many players defined in save file ({playerCount}), check that it did not get corrupted.");
            return default;
        }

        try
        {
            ulong[] players = new ulong[playerCount];
            byte[] buffer = new byte[Math.Min(32, playerCount) * rowSize];
            buffer[0] = firstByte;
            int playerIndex = 0;
            int alreadyThereCount = 1;
            bool waitingOnDigit = true;
            ulong ttl = 0;
            while (true)
            {
                int byteCt = stream.Read(buffer, alreadyThereCount, buffer.Length - alreadyThereCount) + alreadyThereCount;
                if (byteCt <= alreadyThereCount)
                    break;

                alreadyThereCount = 0;
                for (int index = 0; index <= byteCt; ++index)
                {
                    if (index < byteCt)
                    {
                        int digit = buffer[index] - 48;
                        if (digit is >= 0 and <= 9)
                        {
                            waitingOnDigit = false;
                            ttl *= 10;
                            ttl += (ulong)digit;
                            continue;
                        }
                    }

                    if (waitingOnDigit)
                        continue;

                    waitingOnDigit = true;
                    if (playerIndex >= players.Length)
                    {
                        ulong[] old = players;
                        players = new ulong[Math.Max(playerCount + 1, (int)(players.Length * 1.25))];
                        Buffer.BlockCopy(old, 0, players, 0, old.Length * sizeof(ulong));
                    }

                    players[playerIndex] = ttl;
                    ++playerIndex;
                    ttl = 0;
                }
            }

            return new ArraySegment<ulong>(players, 0, playerIndex);
        }
        catch (OutOfMemoryException)
        {
            _plugin.LogWarning($"Too many players defined in save file ({playerCount}), check that it did not get corrupted.");
            return default;
        }
    }
    private unsafe ArraySegment<ulong> ReadBinary(byte firstByte, Stream stream)
    {
        long fileLength = stream.Length;
        if (fileLength % 8 != 0)
        {
            _plugin.LogWarning($"Detected corrupted player save file, size {fileLength} bytes should be divisible by {sizeof(ulong)} bytes. There may be missing players.");
        }

        long playerCount = fileLength / sizeof(ulong);
        if (playerCount > int.MaxValue)
        {
            _plugin.LogWarning($"Too many players defined in save file ({playerCount}), check that it did not get corrupted.");
            return default;
        }

        try
        {
            ulong[] players = new ulong[playerCount];
            byte[] buffer = new byte[Math.Min(32, playerCount) * sizeof(ulong)];
            buffer[0] = firstByte;
            int playerIndex = 0;
            int isFirst = 1;
            fixed (byte* ptr = buffer)
            {
                while (true)
                {
                    int byteCt = stream.Read(buffer, isFirst, buffer.Length - isFirst) + isFirst;
                    if (byteCt < sizeof(ulong))
                        break;

                    isFirst = 0;
                    int plCt = byteCt / sizeof(ulong);
                    for (int i = 0; i < plCt; ++i)
                    {
                        if (playerIndex >= players.Length)
                        {
                            ulong[] old = players;
                            players = new ulong[Math.Max(playerIndex + 1, (int)(players.Length * 1.25d))];
                            Buffer.BlockCopy(old, 0, players, 0, old.Length * sizeof(ulong));
                        }

                        byte* ptr2 = ptr + i * sizeof(ulong);
                        // read big endian
                        ulong steam64 = (uint)ptr2[4] << 24 | (uint)ptr2[5] << 16 | (uint)ptr2[6] << 8 | ptr2[7] |
                                        (ulong)((uint)*ptr2 << 24 | (uint)ptr2[1] << 16 | (uint)ptr2[2] << 8 | ptr2[3]) << 32;
                        players[playerIndex] = steam64;
                        ++playerIndex;
                    }
                }
            }

            return new ArraySegment<ulong>(players, 0, playerIndex);
        }
        catch (OutOfMemoryException)
        {
            Logger.LogWarning($"Too many players defined in save file ({playerCount}), check that it did not get corrupted.");
            return default;
        }
    }
    public void TryAdd(ulong steam64)
    {
        if (HasPreviouslyJoined(steam64))
        {
            return;
        }

        lock (_sync)
        {
            if (_saveIsPlainText != _isPlainTextSaveFileGetter(this))
            {
                AddToList(steam64);
                if (_saveIsPlainText)
                    ConvertToBinary();
                else
                    ConvertToPlainText();
                return;
            }

            using FileStream stream = File.Open(FileLocation, FileMode.OpenOrCreate, _saveIsPlainText ? FileAccess.ReadWrite : FileAccess.Write, FileShare.Read);

            if (_saveIsPlainText)
            {
                stream.Seek(1, SeekOrigin.End);
                int lastByte = stream.ReadByte();
                if (lastByte != -1 && lastByte is >= 48 and < 58 /* digit */)
                {
                    stream.Write(_newLineAsAscii, 0, _newLineAsAscii.Length);
                }

                byte[] asciiData = Encoding.ASCII.GetBytes(steam64.ToString("D17", CultureInfo.InvariantCulture));

                stream.Write(asciiData, 0, asciiData.Length);
            }
            else
            {
                stream.Seek(0, SeekOrigin.End);

                byte[] addBuffer = ThreadUtil.gameThread == Thread.CurrentThread ? _addBuffer : new byte[8];

                WriteSteam64(steam64, addBuffer);

                stream.Write(addBuffer, 0, addBuffer.Length);
            }

            AddToList(steam64);
        }
    }

    private static void WriteSteam64(ulong steam64, byte[] addBuffer)
    {
        unchecked
        {
            addBuffer[0] = (byte)(steam64 >> 56);
            addBuffer[1] = (byte)(steam64 >> 48);
            addBuffer[2] = (byte)(steam64 >> 40);
            addBuffer[3] = (byte)(steam64 >> 32);
            addBuffer[4] = (byte)(steam64 >> 24);
            addBuffer[5] = (byte)(steam64 >> 16);
            addBuffer[6] = (byte)(steam64 >> 8);
            addBuffer[7] = (byte) steam64;
        }
    }

    private void AddToList(ulong steam64)
    {
        if (PreviouslyJoinedPlayers.Array == null || PreviouslyJoinedPlayers.Count == 0)
        {
            PreviouslyJoinedPlayers = new ArraySegment<ulong>(new ulong[4], 0, 1);
            PreviouslyJoinedPlayers.Array![0] = steam64;
        }
        else if (PreviouslyJoinedPlayers.Array.Length - PreviouslyJoinedPlayers.Offset > PreviouslyJoinedPlayers.Count)
        {
            PreviouslyJoinedPlayers = new ArraySegment<ulong>(PreviouslyJoinedPlayers.Array, PreviouslyJoinedPlayers.Offset, PreviouslyJoinedPlayers.Count + 1);
            PreviouslyJoinedPlayers.Array![PreviouslyJoinedPlayers.Offset + PreviouslyJoinedPlayers.Count - 1] = steam64;
        }
        else
        {
            ulong[] newArray = new ulong[Math.Max(PreviouslyJoinedPlayers.Count + 1, (int)(PreviouslyJoinedPlayers.Count * 1.25))];
            Buffer.BlockCopy(PreviouslyJoinedPlayers.Array, PreviouslyJoinedPlayers.Offset, newArray, 0, PreviouslyJoinedPlayers.Count * sizeof(ulong));
            newArray[PreviouslyJoinedPlayers.Count] = steam64;
            PreviouslyJoinedPlayers = new ArraySegment<ulong>(newArray, 0, PreviouslyJoinedPlayers.Count + 1);
        }
    }
    public bool HasPreviouslyJoined(ulong steam64)
    {
        return PreviouslyJoinedPlayers.Array != null
               && Array.IndexOf(PreviouslyJoinedPlayers.Array, steam64, PreviouslyJoinedPlayers.Offset, PreviouslyJoinedPlayers.Count) != -1;
    }
}