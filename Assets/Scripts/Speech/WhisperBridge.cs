using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class WhisperBridge : IDisposable {
    public WhisperBridge() {
        // todo start python as subprocess
    }


    public string RequestSTT(AudioClip clip) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        //var guid = Guid.NewGuid().ToString();
        //var path = Path.Combine(Application.temporaryCachePath, $"sr-{guid[..8]}.wav");
        var path = Path.Combine(Application.temporaryCachePath, "sr-swap.wav");
        SaveWav(path, clip);

        var _client = new SrClient();
        _client.Send(path);
        var text = _client.Read();
        _client.Dispose();

        return text;
#else
        return Extensions.BypassToken;
#endif
    }


    public void Dispose() {
        // todo end python server process
    }


    // based on https://gist.github.com/darktable/2317063 with modifications
    public static string SaveWav(string path, AudioClip clip) {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using var fs = new FileStream(path, FileMode.Create);
        Debug.Log($"Recording saved to {path}");

        // header placeholder
        const int header_size = 44;
        for(var i = 0; i < header_size; ++i)
            fs.WriteByte(0);

        // data
        using var ms = new MemoryStream();
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        var asInt16 = new short[samples.Length];
        var asInt8 = new byte[samples.Length * 2];
        const float rescaleFactor = 32767;
        for(var i = 0; i < samples.Length; ++i)
            asInt16[i] = (short)(samples[i] * rescaleFactor);
        Buffer.BlockCopy(asInt16, 0, asInt8, 0, asInt8.Length);
        ms.Write(asInt8, 0, asInt8.Length);
        ms.WriteTo(fs);

        // wav header
        fs.Seek(0, SeekOrigin.Begin);
        var chunkID = Encoding.ASCII.GetBytes("RIFF");
        fs.Write(chunkID, 0, 4);
        var chunkSize = BitConverter.GetBytes((int)(fs.Length - 8));
        fs.Write(chunkSize, 0, 4);
        var format = Encoding.ASCII.GetBytes("WAVE");
        fs.Write(format, 0, 4);
        var subChunk1ID = Encoding.ASCII.GetBytes("fmt ");
        fs.Write(subChunk1ID, 0, 4);
        var subChunk1Size = BitConverter.GetBytes(16);
        fs.Write(subChunk1Size, 0, 4);
        var audioFormat = BitConverter.GetBytes((short)1);
        fs.Write(audioFormat, 0, 2);
        var numChannels = BitConverter.GetBytes((short)clip.channels);
        fs.Write(numChannels, 0, 2);
        var sampleRate = BitConverter.GetBytes(clip.frequency);
        fs.Write(sampleRate, 0, 4);
        const int bytesPerSample = 2;
        var byteRate = BitConverter.GetBytes(clip.frequency * bytesPerSample * clip.channels);
        fs.Write(byteRate, 0, 4);
        var blockAlign = BitConverter.GetBytes((short)(clip.channels * 2));
        fs.Write(blockAlign, 0, 2);
        var bitsPerSample = BitConverter.GetBytes((short)16);
        fs.Write(bitsPerSample, 0, 2);
        var subChunk2ID = Encoding.ASCII.GetBytes("data");
        fs.Write(subChunk2ID, 0, 4);
        var subChunk2Size = BitConverter.GetBytes(clip.samples * bytesPerSample * clip.channels);
        fs.Write(subChunk2Size, 0, 4);

        return path;
    }
}


internal class SrClient : IDisposable {
    private static readonly int port = 55442;
    private static readonly uint magic = 0xdeadbeef;

    private Socket socket;


    public SrClient() {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        socket.Connect(endPoint);
        Debug.Log("socket connected");
    }


    public void Send(string data) {
        var buf = Encoding.UTF8.GetBytes(data);
        socket.Send(BitConverter.GetBytes(magic));
        socket.Send(BitConverter.GetBytes(buf.Length));
        socket.Send(buf);
    }


    public string Read() {
        var buf = new byte[4];
        socket.Receive(buf);
        if(BitConverter.ToUInt32(buf) != magic)
            return "";
        socket.Receive(buf);
        var len = BitConverter.ToUInt32(buf);
        if(len == 0)
            return "";
        buf = new byte[len];
        socket.Receive(buf);
        var data = Encoding.UTF8.GetString(buf);
        return data;
    }


    public void Dispose() {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        socket.Dispose();
        socket = null;
    }
}
