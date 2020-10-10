using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimplyNats.Protocol
{
  internal class Connect
  {
    [JsonPropertyName("verbose")]
    public bool? Verbose { get; set; } // Turns on +OK protocol acknowledgements.

    [JsonPropertyName("pedantic")]
    public bool? Pedantic { get; set; } // Turns on additional strict format checking, e.g. for properly formed subjects

    [JsonPropertyName("tls_required")]
    public bool? TlsRequired { get; set; } // Indicates whether the client requires an SSL connection.

    [JsonPropertyName("auth_token")]
    public string? AuthToken { get; set; } // Client authorization token (if auth_required is set)

    [JsonPropertyName("user")]
    public string? User { get; set; }= null; //  Connection username (if auth_required is set)

    [JsonPropertyName("pass")]
    public string? Pass { get; set; }= null; // Connection password (if auth_required is set)

    [JsonPropertyName("name")]
    public string? Name { get; set; }// Optional client name

    [JsonPropertyName("lang")]
    public string? Lang { get; set; } // The implementation language of the client.

    [JsonPropertyName("version")]
    public string? Version { get; set; } // The version of the client.

    [JsonPropertyName("protocol")]
    public int? Protocol { get; set; }
      // optional int. Sending 0 (or absent) indicates client supports original protocol.
                               // Sending 1 indicates that the client supports dynamic reconfiguration of cluster
                               // topology changes by asynchronously receiving INFO messages with known servers it
                               // can reconnect to.

    [JsonPropertyName("echo")]
    public bool? Echo { get; set; } // echo: Optional boolean. If set to true, the server (version 1.2.0+) will not send
                                // originating messages from this connection to its own subscriptions. Clients should
                                // set this to true only for server supporting this feature, which is when proto in
                                // the INFO protocol is set to at least 1.

    [JsonPropertyName("sig")]
    public string? Sig { get; set; }
      // In case the server has responded with a nonce on INFO, then a NATS client must use
      // this field to reply with the signed nonce.

    [JsonPropertyName("jwt")]
    public string? Jwt { get; set; }
      // The JWT that identifies a user permissions and acccount.

    public string ToJson()
    {
      return JsonSerializer.Serialize(this, new JsonSerializerOptions
      {
        IgnoreNullValues = true
      });
    }

    public static Connect FromJson(string json)
    {
      return JsonSerializer.Deserialize<Connect>(json, new JsonSerializerOptions{});
    }

  }
}