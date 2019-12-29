using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NeteaseCloudMusicApi;
using core;
using netease;
using secret;
namespace WPF_Player.NeateaseAPI
{
    class API
    {
        public async void getUserListAsync(string path)
        {
            string uIdPath = path + "uid.ini";
            while (!File.Exists(uIdPath)) { }
            StreamReader listIdReader = new StreamReader(uIdPath);
            String uid = listIdReader.ReadLine();
            CloudMusicApi api = new CloudMusicApi();
            bool isOk;
            JObject json = new JObject();
            Dictionary<String, String> param = new Dictionary<string, string> { { "uid", uid } };
            (isOk, json) = await api.RequestAsync(CloudMusicApiProviders.UserPlaylist, param);
            var playList = json.SelectTokens("playlist[*]").ToDictionary(t => t["id"], t => t["name"]);
            //foreach (var item in playList)
            //{
            //    Console.WriteLine(item.Key + ":" + item.Value);
            //}
            string filePath = path + "playlist.json";
            StreamWriter writer = File.CreateText(filePath);
            writer.Write(JObject.FromObject(playList));
            writer.Flush();
            String defalutPlayId = playList.FirstOrDefault().Key.ToString();
            String listIdPath = path + "listid.ini";
            writer = File.CreateText(listIdPath);
            writer.Write(defalutPlayId);
            writer.Flush();
            writer.Close();
        }

        public async void getSongs(String path)
        {
            string listIdPath = path + "listid.ini";
            StreamReader listIdReader = new StreamReader(listIdPath);
            String listId = listIdReader.ReadLine();
            CloudMusicApi api = new CloudMusicApi();
            bool isOk;
            JObject json = new JObject();
            Dictionary<String, String> param = new Dictionary<string, string> { { "id", listId } };
            (isOk, json) = await api.RequestAsync(CloudMusicApiProviders.PlaylistDetail, param);
            var songList = json.SelectTokens("playlist.tracks[*]").ToDictionary(t => t["id"], t => t["name"]);
            //foreach (var item in songList)
            //{
            //    Console.WriteLine(item.Key + ":" + item.Value);
            //}
            string filePath = path + "songs.json";
            StreamWriter writer = File.CreateText(filePath);
            writer.Write(JObject.FromObject(songList));
            writer.Flush();
            writer.Close();
        }
    }
};