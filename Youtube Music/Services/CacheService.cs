using System;
using System.IO;
using System.Text;

namespace Youtube_Music.Services
{
    public class CacheService
    {
        public static char[] ReadChars(string filename, int count)
        {
            using var stream = File.OpenRead(filename);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            char[] buffer = new char[count];
            int n = reader.ReadBlock(buffer, 0, count);

            char[] result = new char[n];

            Array.Copy(buffer, result, n);

            return result;
        }

        public CacheService()
        {
            //Create cache directory
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\YoutubeMusicNET"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\YoutubeMusicNET");

            if (!File.Exists(RecentlyPlayedPath)) File.WriteAllText(RecentlyPlayedPath, "[]");
            else
            {
                var json = ReadChars(RecentlyPlayedPath, 1);
                if (json.Length == 0 || json[0] != '[') File.WriteAllText(RecentlyPlayedPath, "[]");
            }
            if (!File.Exists(LikedSongsPath)) File.WriteAllText(LikedSongsPath, "[]");
            else
            {
                var json = ReadChars(LikedSongsPath, 1);
                if (json.Length == 0 || json[0] != '[') File.WriteAllText(LikedSongsPath, "[]");
            }
        }

        public string SaveFolder { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\YoutubeMusicNET\\";

        public string RecentlyPlayedPath { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\YoutubeMusicNET\\RecentlyPlayed.json";

        public string LikedSongsPath { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\YoutubeMusicNET\\LikedSongs.json";
    }
}
