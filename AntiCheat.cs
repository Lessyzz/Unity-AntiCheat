using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AntiCheat : MonoBehaviour
{   
    public bool IsCheatDetected { get; private set; }

    #region Speed Hack Variables

    private float lastRealTime;
    private float lastGameTime;
    private int violations;

    #endregion

    #region Memory Manipulation Handlers

    public static event Action<string> OnCheatDetected;

    public static void TriggerCheat(string msg) => OnCheatDetected?.Invoke(msg);

    #endregion

    void Awake() => FileFolderChecker();

    private void Start()
    {
        AntiCheat.OnCheatDetected += HandleCheat;
        lastRealTime = Time.realtimeSinceStartup;
        lastGameTime = Time.time;
        InvokeRepeating("SpeedHackChecker", .5f, .5f);
    }

    // Suspicious File/Folder Detection
    private void FileFolderChecker()
    {
        if (!Application.isEditor)
        {
            string[] suspiciousDlls = { "dobby.dll", "version.dll", "versions.dll", "winhttp:dll" };
            string rootDirectory = Directory.GetParent(Application.dataPath).FullName;

            foreach (string dll in suspiciousDlls)
            {
                if (File.Exists(Path.Combine(rootDirectory, dll)))
                {
                    AntiCheat.TriggerCheat($"Suspicious DLL detected: {dll}");
                    Application.Quit();
                }
            }
            
            if (Directory.Exists(Path.Combine(rootDirectory, "Mods")) || 
                Directory.Exists(Path.Combine(rootDirectory, "BepInEx")))
            {
                AntiCheat.TriggerCheat("Suspicious folder detected!");
                Application.Quit();
            }
        }
    }

    // Triggered when a cheat is detected
    private void HandleCheat(string msg)
    {
        Time.timeScale = 0f;
        print($"Cheat Detected: {msg}");
        IsCheatDetected = true;
        Application.Quit();
    }

    void SpeedHackChecker()
    {
        float currentRealTime = Time.realtimeSinceStartup;
        float currentGameTime = Time.time;
        
        float realDelta = currentRealTime - lastRealTime;
        float gameDelta = currentGameTime - lastGameTime;
        
        // Fps drop check
        if (realDelta < 0.3f) return;
        
        float speedRatio = gameDelta / realDelta;
        
        // Speed hack detection
        if (speedRatio > 1.05f)
        {
            violations++;
            
            // Speed hack detected!
            if (violations >= 3)
            {
                Application.Quit();
            }
        }
        else
        {
            violations = Mathf.Max(0, violations - 1);
        }
        
        lastRealTime = currentRealTime;
        lastGameTime = currentGameTime;
    }

}

#region Secure Types

public static class SecureRandom
{
    private static readonly System.Random _rng = new System.Random();

    public static int NextInt(int min, int max) => _rng.Next(min, max);
    public static long NextLong()
    {
        byte[] buffer = new byte[8];
        _rng.NextBytes(buffer);
        return BitConverter.ToInt64(buffer, 0);
    }

    public static byte NextByte(byte min = 1, byte max = 255) =>
        (byte)_rng.Next(min, max);
}

[Serializable]
public struct SecureInt
{
    private int _encryptedValue;
    private int _fakeValue;
    private int _key;

    public SecureInt(int value)
    {
        _key = SecureRandom.NextInt(int.MinValue, int.MaxValue);
        _encryptedValue = value ^ _key;
        _fakeValue = value;
    }

    public int Value
    {
        get
        {
            int decrypted = _encryptedValue ^ _key;
            if (decrypted != _fakeValue)
                AntiCheat.TriggerCheat("SecureInt cheat detected!");
            return decrypted;
        }
        set
        {
            _key = SecureRandom.NextInt(int.MinValue, int.MaxValue);
            _encryptedValue = value ^ _key;
            _fakeValue = value;
        }
    }

    public static implicit operator int(SecureInt s) => s.Value;
    public static implicit operator SecureInt(int v) => new SecureInt(v);

    public override string ToString() => Value.ToString();
    public string ToString(string format) => Value.ToString(format);
}

[Serializable]
public struct SecureFloat
{
    private int _encryptedValue;
    private float _fakeValue;
    private int _key;

    public SecureFloat(float value)
    {
        _key = SecureRandom.NextInt(int.MinValue, int.MaxValue);
        _encryptedValue = BitConverter.ToInt32(BitConverter.GetBytes(value), 0) ^ _key;
        _fakeValue = value;
    }

    public float Value
    {
        get
        {
            float decrypted = BitConverter.ToSingle(BitConverter.GetBytes(_encryptedValue ^ _key), 0);
            if (Math.Abs(decrypted - _fakeValue) > 0.0001f)
                AntiCheat.TriggerCheat("SecureFloat cheat detected!");
            return decrypted;
        }
        set
        {
            _key = SecureRandom.NextInt(int.MinValue, int.MaxValue);
            _encryptedValue = BitConverter.ToInt32(BitConverter.GetBytes(value), 0) ^ _key;
            _fakeValue = value;
        }
    }

    public static implicit operator float(SecureFloat s) => s.Value;
    public static implicit operator SecureFloat(float v) => new SecureFloat(v);

    public override string ToString() => Value.ToString();
    public string ToString(string format) => Value.ToString(format);
}

[Serializable]
public struct SecureLong
{
    private long _encryptedValue;
    private long _fakeValue;
    private long _key;

    public SecureLong(long value)
    {
        _key = SecureRandom.NextLong();
        _encryptedValue = value ^ _key;
        _fakeValue = value;
    }

    public long Value
    {
        get
        {
            long decrypted = _encryptedValue ^ _key;
            if (decrypted != _fakeValue)
                AntiCheat.TriggerCheat("SecureLong cheat detected!");
            return decrypted;
        }
        set
        {
            _key = SecureRandom.NextLong();
            _encryptedValue = value ^ _key;
            _fakeValue = value;
        }
    }

    public static implicit operator long(SecureLong s) => s.Value;
    public static implicit operator SecureLong(long v) => new SecureLong(v);

    public override string ToString() => Value.ToString();
    public string ToString(string format) => Value.ToString(format);
}

[Serializable]
public struct SecureBool
{
    private byte _encrypted;
    private bool _fake;
    private byte _key;

    public SecureBool(bool value)
    {
        _key = SecureRandom.NextByte();
        _encrypted = (byte)((value ? 1 : 0) ^ _key);
        _fake = value;
    }

    public bool Value
    {
        get
        {
            bool decrypted = (_encrypted ^ _key) != 0;
            if (decrypted != _fake)
                AntiCheat.TriggerCheat("SecureBool cheat detected!");
            return decrypted;
        }
        set
        {
            _key = SecureRandom.NextByte();
            _encrypted = (byte)((value ? 1 : 0) ^ _key);
            _fake = value;
        }
    }

    public static implicit operator bool(SecureBool s) => s.Value;
    public static implicit operator SecureBool(bool v) => new SecureBool(v);


    public override string ToString() => Value.ToString();
}

[Serializable]
public struct SecureString
{
    private string _encrypted;
    private string _fake;
    private byte _key;

    public SecureString(string value)
    {
        _key = SecureRandom.NextByte();
        _encrypted = Encrypt(value, _key);
        _fake = value;
    }

    public string Value
    {
        get
        {
            string decrypted = Decrypt(_encrypted, _key);
            if (decrypted != _fake)
                AntiCheat.TriggerCheat("SecureString cheat detected!");
            return decrypted;
        }
        set
        {
            _key = SecureRandom.NextByte();
            _encrypted = Encrypt(value, _key);
            _fake = value;
        }
    }

    private static string Encrypt(string input, byte key)
    {
        var sb = new StringBuilder();
        foreach (char c in input)
            sb.Append((char)(c ^ key));
        return sb.ToString();
    }

    private static string Decrypt(string input, byte key) => Encrypt(input, key);

    public static implicit operator string(SecureString s) => s.Value;
    public static implicit operator SecureString(string v) => new SecureString(v);
    public override string ToString() => Value;
}

[Serializable]
public struct SecureDateTime
{
    private long _encrypted;
    private DateTime _fake;
    private long _key;

    public SecureDateTime(DateTime value)
    {
        _key = SecureRandom.NextLong();
        _encrypted = value.Ticks ^ _key;
        _fake = value;
    }

    public DateTime Value
    {
        get
        {
            DateTime decrypted = new DateTime(_encrypted ^ _key);
            if (decrypted != _fake)
                AntiCheat.TriggerCheat("SecureDateTime cheat detected!");
            return decrypted;
        }
        set
        {
            _key = SecureRandom.NextLong();
            _encrypted = value.Ticks ^ _key;
            _fake = value;
        }
    }

    public static implicit operator DateTime(SecureDateTime s) => s.Value;
    public static implicit operator SecureDateTime(DateTime v) => new SecureDateTime(v);

    public override string ToString() => Value.ToString();
    public string ToString(string format) => Value.ToString(format);
}

#endregion