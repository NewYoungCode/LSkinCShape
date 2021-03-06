﻿using MusicPlay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;




public static class http
{

    /// <summary>
    /// 从本地获取数据
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<Song> LoadSong(string path)
    {
        List<Song> list = new List<Song>();

        foreach (string file in Directory.GetFiles(path))
        {
            int index = file.LastIndexOf(".");
            string extname = file.Substring(index, file.Length - index);


            if (extname != ".mp3" && extname != ".mp4") continue;

            Song s = new Song();
            s.Hash = null;
            s.Extname = extname;
            s.Play_url = file;
            s.Filename = file.Substring(file.LastIndexOf(@"\") + 1, file.Length - file.LastIndexOf(@"\") - extname.Length - 1);

            if (extname == ".mp4") s.MvHash = "local";
            if (s.Filename.Contains("-"))
            {
                s.Singername = s.Filename.Substring(0, s.Filename.IndexOf("-") - 1);
            }
            else
            {
                s.Singername = "网络歌手";
            }

            list.Add(s);
        }
        return list;
    }

    public static string getJsonText(string urTxtl)
    {
        WebRequest rq = new WebRequest();
        return rq.Get(urTxtl);

    }
    //WebClient wc = new WebClient();
    //    wc.Credentials = CredentialCache.DefaultCredentials;
    //    Encoding encoding = Encoding.GetEncoding("UTF-8");
    //    Byte[] data = wc.DownloadData(url);
    //    wc.Dispose();
    //    string finalStr = encoding.GetString(data);
    //    return finalStr;

    public static string getUrl(string JsonText)
    {

        try
        {

            JsonValue obj = JsonObject.Parse(JsonText);
            string url = obj["url"].ToString();
            url = url.Replace(@"""", "");
            return url;
        }
        catch (Exception)
        {


        }
        return null;

    }


    public static string[] GetMvList(string JsonText)
    {
        JsonValue obj = JsonObject.Parse(JsonText);
        JsonValue data = null;
        try
        {
            data = obj["mvdata"];

        }
        catch (Exception)
        {
            return new string[0];
        }

        string[] type = { "le", "rq", "sq" };

        List<string> lists = new List<string>();

        foreach (string item in type)
        {
            try
            {
                string url = item + "," + data[item]["downurl"].ToString();
                lists.Add(url);
            }
            catch (Exception)
            {
                continue;
            }
        }

        string[] strs = new string[lists.Count];
        foreach (var item in lists)
        {
            strs[lists.IndexOf(item)] = item;
        }

        return strs;
    }

    public static Song GetSong(string hash, Song s)
    {
        string url = kugouApi.ReplaceSongInfoUrl(hash);
        string jsonText = getJsonText(url);
        string xx = getJsonText(kugouApi.ReplaceSongInfoUrl(s.Hash));
        JsonValue data = JsonObject.Parse(jsonText);
        s.Play_url = data["url"].ToString();
        s.SingerHeadimg = data["imgUrl"].ToString().Replace("{size}", 100 + "");
        string[] mvList = GetMvList(http.getJsonText(kugouApi.ReplaceMVUrl(s.MvHash)));
        s.Mv = mvList;
        return s;
    }

    public static List<string> GetSingerPhoto(string singerName)
    {
        List<string> photoList = new List<string>();
        try
        {
            JsonValue obj = JsonObject.Parse(getJsonText(kugouApi.ReplaceSingerUrl(singerName)));
            foreach (var item in obj["array"])
            {
                if (item.Value.ContainsKey("wpurl"))
                {
                    photoList.Add(item.Value["wpurl"].ToString().Replace("\\", "").Replace(@"""", ""));
                }
            }
        }
        catch (Exception) { }

        return photoList;
    }

    public static List<Song> GetList(string JsonText)
    {

        if (JsonText.Contains("jQuery"))
        {

            int startIndex = JsonText.IndexOf("(");
            int length = JsonText.Length;
            JsonText = JsonText.Substring(startIndex + 1, length - (startIndex + 1 + 2));
            JsonText = JsonText.Replace("<em>", "");
            JsonText = JsonText.Replace("<\\/em>", "");
            JsonText = JsonText.Replace("<\\/em>", "");



        }

        JsonValue obj = JsonObject.Parse(JsonText);
        int total = obj["data"]["lists"].Count;
        JsonValue data = obj["data"]["lists"];

        List<Song> songs = new List<Song>();
        Song s = null;
        for (int i = 0; i < total; i++)
        {

            string extname = data[i]["ExtName"].ToString();
            string filename = data[i]["FileName"].ToString();
            string songname = data[i]["SongName"].ToString();
            string hash = data[i]["FileHash"].ToString();
            string duration = data[i]["Duration"].ToString();
            string durationStr = data[i]["Duration"].ToString();
            string singername = data[i]["SingerName"].ToString();
            string MvHash = data[i]["MvHash"].ToString();

            songname = songname.Replace(@"""", "");

            filename = filename.Replace(@"\", "");
            filename = filename.Replace(@"/", "");

            extname = extname.Replace(@"""", "");
            filename = filename.Replace(@"""", "");
            hash = hash.Replace(@"""", "");
            durationStr = durationStr.Replace(@"""", "");
            singername = singername.Replace(@"""", "");
            MvHash = MvHash.Replace(@"""", "");

            s = new Song();
            s.MvHash = MvHash;
            s.Filename = filename;
            s.Duration = duration;
            s.DurationStr = durationStr;
            s.Singername = singername;
            s.Hash = hash;
            s.Extname = extname;
            s.Songname = songname;
            songs.Add(s);
        }

        return songs;

    }
    public static Lrc getLrc(string LrcJsonText)
    {
        Lrc l = new Lrc();
        try
        {
            JsonValue obj = JsonObject.Parse(LrcJsonText);
            string id = obj["candidates"][0]["id"].ToString();
            string accesskey = obj["candidates"][0]["accesskey"].ToString();
            id = id.Replace(@"""", "");
            accesskey = accesskey.Replace(@"""", "");
            l.Accesskey = accesskey;
            l.Id = id;
            return l;
        }
        catch (Exception)
        {


        }
        return null;


    }

    public static string Getcontent(string JsonText)
    {
        Lrc l = new Lrc();
        try
        {
            JsonValue obj = JsonObject.Parse(JsonText);

            string content = obj["content"].ToString();

            content = content.Replace(@"""", "");

            return content;
        }
        catch (Exception)
        {


        }
        return null;


    }
}
