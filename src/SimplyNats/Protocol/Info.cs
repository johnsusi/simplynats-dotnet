using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("SimplyNats.Test")]
namespace SimplyNats.Protocol
{

  internal class Info
  {
    [JsonPropertyName("server_id")]
    public string? ServerId { get; set; } // The unique identifier of the NATS server

    [JsonPropertyName("server_name")]
    public string? ServerName { get; set; } // The unique identifier of the NATS server

    [JsonPropertyName("version")]
    public string? Version { get; set; } // The version of the NATS server
    [JsonPropertyName("go")]

    public string? Go { get; set; } // The version of golang the NATS server was built with
    [JsonPropertyName("host")]
    public string? Host { get; set; } // The IP address used to start the NATS server, by default this will be 0.0.0.0 and can be configured with -client_advertise host:port

    [JsonPropertyName("port")]
    public int Port { get; set; } // The port number the NATS server is configured to listen on
    [JsonPropertyName("max_payload")]

    public long MaxPayload { get; set; } // Maximum payload size, in bytes, that the server will accept from the client.
    [JsonPropertyName("proto")]
    public int Proto { get; set; } // An integer indicating the protocol version of the server. The server version 1.2.0 sets this to 1 to indicate that it supports the "Echo" feature.

    [JsonPropertyName("client_id")]
    public ulong ClientId { get; set; } // An optional unsigned integer (64 bits) representing the internal client identifier in the server. This can be used to filter client connections in monitoring, correlate with error logs, etc...
    [JsonPropertyName("client_ip")]

    public string? ClientIp { get; set; } // An optional unsigned integer (64 bits) representing the internal client identifier in the server. This can be used to filter client connections in monitoring, correlate with error logs, etc...
    [JsonPropertyName("auth_required")]
    public bool AuthRequired { get; set; } = false; // If this is set, then the client should try to authenticate upon connect.

    [JsonPropertyName("tls_required")]
    public bool TlsRequired { get; set; } = false; // If this is set, then the client must perform the TLS/1.2 handshake. Note, this used to be ssl_required and has been updated along with the protocol from SSL to TLS.
    [JsonPropertyName("tls_verify")]

    public bool TlsVerify { get; set; } = false; // If this is set, the client must provide a valid certificate during the TLS handshake.
    [JsonPropertyName("connect_urls")]
    public IList<string>? ConnectUrls { get; set; } // An optional list of server urls that a client can connect to.

    [JsonPropertyName("ldm")]
    public bool Ldm { get; set; } = false; //:If the server supports Lame Duck Mode notifications, and the current server has transitioned to lame duck, ldm will be set to true.

    public string ToJson()
    {
      return JsonSerializer.Serialize(this, new JsonSerializerOptions{});
    }

    public static Info FromJson(string json)
    {
      return JsonSerializer.Deserialize<Info>(json, new JsonSerializerOptions{});
    }


  }
}