using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace CoinFlips.Models;

public class Webhook
{
    public string? username { get; set; }
    public string? avatar_url { get; set; }
    public string? content { get; set; }
    public List<Embed>? embeds { get; set; }

    public void send(string webhook_url)
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        var wr = WebRequest.Create(webhook_url);
        wr.ContentType = "application/json";
        wr.Method = "POST";

        using (var sw = new StreamWriter(wr.GetRequestStream()))
        {
            sw.WriteLine(json);
        }
        wr.GetResponse();
    }
}

public class Embed
{
    public string? title { get; set; }
    public int color { get; set; }
    public string? description { get; set; }
    public DateTime timestamp { get; set; }
    public string? url { get; set; }
    public Dictionary<string, string>? author { get; set; }
    public Dictionary<string, string>? image { get; set; }
    public Dictionary<string, string>? thumbnail { get; set; }
    public Dictionary<string, string>? footer { get; set; }
    public List<Field>? fields { get; set; }
}

public class Field
{
    public string? name { get; set; }
    public string? value { get; set; }
    public bool inline { get; set; }
}