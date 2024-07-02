using System.Text.Json;
using System.Text.Json.Nodes;

namespace Shared;

public class HalResource
{
  private readonly Dictionary<string, HalLink> _links = [];
  private readonly Dictionary<string, HalResource> _embedded = [];
  private readonly Dictionary<string, IList<HalResource>> _embeddedLists = [];
  private readonly JsonObject _properties = [];

  private HalResource() { }

  public static HalResource Create(string? selfUrl = null)
  {
    var resource = new HalResource();
    if (selfUrl != null)
      resource.AddLink("self", selfUrl);

    return resource;
  }

  private void SetProperties(JsonObject json)
  {
    foreach (var property in json)
    {
      _properties.Add(property.Key, property.Value?.DeepClone());
    }
  }

  public HalResource AddProperty(string propertyKey, JsonNode? value)
  {
    _properties.Add(propertyKey, value);
    return this;
  }

  public T? GetProperty<T>(string propertyKey)
  {
    var value = _properties[propertyKey];
    return value == null ? default : value.GetValue<T>();
  }

  public JsonObject? GetObjectProperty(string propertyKey)
  {
    var value = _properties[propertyKey];
    return value?.AsObject();
  }

  public HalResource AddLink(string rel, string url)
  {
    _links.Add(rel, new HalLink(url));
    return this;
  }

  public string GetHref(string rel)
  {
    return _links[rel].Href;
  }

  public HalResource AddEmbedded(string rel, HalResource resource)
  {
    _embedded.Add(rel, resource);
    return this;
  }

  public HalResource AddEmbeddedList(string rel, IList<HalResource> resources)
  {
    _embeddedLists.Add(rel, resources);
    return this;
  }

  public IList<HalResource> GetEmbeddedList(string rel)
  {
    return _embeddedLists[rel];
  }

  public string ToJson()
  {
    return ToJsonObject().ToJsonString();
  }

  private JsonObject ToJsonObject()
  {
    var jsonResource = new JsonObject();

    if (_links.Count > 0)
    {
      var linksResource = new JsonObject();
      foreach (var (rel, link) in _links)
      {
        linksResource.Add(rel, new JsonObject { { "href", link.Href } });
      }

      jsonResource.Add("_links", linksResource);
    }

    if (_embedded.Count > 0)
    {
      var embeddedResource = new JsonObject();
      foreach (var (rel, embedded) in _embedded)
      {
        embeddedResource.Add(rel, embedded.ToJsonObject());
      }

      jsonResource.Add("_embedded", embeddedResource);
    }

    if (_embeddedLists.Count > 0)
    {
      var embeddedResource = new JsonObject();
      foreach (var (rel, embeddedList) in _embeddedLists)
      {
        embeddedResource.Add(rel, new JsonArray(embeddedList.Select(embedded => embedded.ToJsonObject()).ToArray()));
      }

      jsonResource.Add("_embedded", embeddedResource);
    }

    foreach (var (key, value) in _properties)
    {
      jsonResource.Add(key, value?.DeepClone());
    }

    return jsonResource;
  }

  public static HalResource FromJson(string jsonString)
  {
    var json = JsonNode.Parse(jsonString)!.AsObject();
    return FromJsonObject(json);
  }

  private static HalResource FromJsonObject(JsonObject json)
  {
    var resource = Create();
    var links = json["_links"]?.AsObject();
    foreach (var link in links ?? [])
    {
      var linkObject = link.Value!.AsObject();
      resource.AddLink(link.Key, linkObject["href"]!.GetValue<string>());
    }

    var embedded = json["_embedded"]?.AsObject();
    foreach (var embed in embedded ?? [])
    {
      var value = embed.Value!;
      if (value.GetValueKind() == JsonValueKind.Array)
      {
        var embedList = value.AsArray();
        resource.AddEmbeddedList(embed.Key, embedList.Select(obj => FromJsonObject(obj!.AsObject())).ToList());
      }
      else
      {
        resource.AddEmbedded(embed.Key, FromJsonObject(value.AsObject()));
      }
    }

    json.Remove("_links");
    json.Remove("_embedded");

    resource.SetProperties(json);

    return resource;
  }
}

public class HalLink(string href)
{
  public string Href { get; } = href;
}