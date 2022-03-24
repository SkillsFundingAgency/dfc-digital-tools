// See https://aka.ms/new-console-template for more information

using Microsoft.Azure.Cosmos;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

Console.WriteLine("Neo4j Endpoint: Please enter the neo4j endpoint (e.g. bolt://HOSTNAME:7687)");
var neo4JEndpoint = Console.ReadLine()!.Trim();

Console.WriteLine("Neo4j DB Name: Please enter the neo4j db name (e.g. published)");
var neo4JDbName = Console.ReadLine()!.Trim();

Console.WriteLine("Neo4j DB Name: Please enter the neo4j username");
var neo4JUsername = Console.ReadLine()!.Trim();

Console.WriteLine("Neo4j DB Name: Please enter the neo4j password");
var neo4JPassword = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos DB Connection string: Please enter the Cosmos db connection string (e.g. AccountEndpoint=https://HOSTNAME:443/;AccountKey=KEY;)");
var cosmosConnectionString = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos DB Database name: Please enter the Cosmos db connection string (e.g. dev)");
var cosmosDbDatabaseName = Console.ReadLine()!.Trim();

Console.WriteLine("Cosmos DB Container name: Please enter the Cosmos db container name to write to  (e.g. published)");
var cosmosDbContainerName = Console.ReadLine()!.Trim();

Console.WriteLine("Please review summary below");
Console.WriteLine();

Console.WriteLine($"Neo4j endpoint - {neo4JEndpoint}");
Console.WriteLine($"Neo4j dbname - {neo4JDbName}");
Console.WriteLine($"Neo4j username - {neo4JUsername}");
Console.WriteLine($"Neo4j password - {neo4JPassword}");
Console.WriteLine($"Cosmos Db connection string - {cosmosConnectionString}");
Console.WriteLine($"Cosmos Db db name - {cosmosDbDatabaseName}");
Console.WriteLine($"Cosmos Db container name - {cosmosDbContainerName}");

Console.WriteLine();
Console.WriteLine("Press Y to proceed - or any other key to quite");

var y = Console.ReadKey().KeyChar.ToString();
if (!y!.Equals("y", StringComparison.InvariantCultureIgnoreCase))
{
    Console.WriteLine();
    Console.WriteLine("Y not hit - quitting");
    return;
}

var cosmosDb = new CosmosClient(cosmosConnectionString);
Console.WriteLine("About to request all the labels");

var exclusions = new List<string>
{
    "Resource",
    "_GraphConfig",
    "_NsPrefDef",
    "skosxl__Label",
    "skos__Concept",
    "skos__ConceptScheme",
    "esco__NodeLiteral",
    "esco__AssociationObject",
    "esco__MemberConcept",
    "esco__Skill",
    "esco__Label",
    "esco__Occupation",
    "esco-rp__Regulation",
    "esco__ConceptScheme",
    "esco__Structure",
    "esco__LabelRole",
    "DynamicTitlePrefix"
};

var contentTypes = (await RunQuery(new Query("call db.labels()"), neo4JDbName, true))
    .Select(label => label.Values.Values.First().As<string>())
    .Where(label => !exclusions.Contains(label))
    .ToList();

Console.WriteLine($"{contentTypes.Count} labels received (after filtering). Looping through labels now to fetch data.");
var count = 1;
var total = contentTypes.Count;

foreach (var contentType in contentTypes)
{
    await SaveDocumentsWithoutRelationships(contentType);
    await AddContHasLinks(contentType);
    await AddIncomingLinks(contentType);

    count++;
}

Console.WriteLine("Finished processing.");

IDriver GetNeo4JDriver()
{
    return GraphDatabase.Driver(neo4JEndpoint,AuthTokens.Basic(neo4JUsername, neo4JPassword));
}

async Task AddIncomingLinks(string contentType)
{
    var upwardsRelationshipData = await RunQuery(new Query($"match (a:{contentType})<-[r]-(b) return a.uri, b.uri"), neo4JUsername, true);
    Console.WriteLine($"{upwardsRelationshipData.Count} upward relationship records found for {contentType} - overall {count}c of {total}.");
    var innerCount = 1;

    var grouped = upwardsRelationshipData.GroupBy(x => (string) x.Values["a.uri"]).ToList();
    
    foreach (var relationshipDataRows in grouped)
    {
        var leftUri = relationshipDataRows.Key;
        var (_, id, _) = GetContentTypeAndId(leftUri);
        
        Console.WriteLine($"Fetching document for {contentType} ({innerCount} of {grouped.Count}) - overall {count}c of {total}.");
        var document = await GetFromCosmosDb(id, contentType.ToLower());
        
        var links = (document["_links"] as JObject)!.ToObject<Dictionary<string, object>>();
        var curies = (links?["curies"] as JArray)!.ToObject<List<Dictionary<string, object>>>();
        var incomingPosition = curies!.FindIndex(curie =>
            (string)curie["name"] == "incoming");
                
        var incomingObject = curies.Count > incomingPosition ? curies[incomingPosition] : null;

        if (incomingObject == null)
        {
            throw new MissingFieldException("Incoming property missing");
        }
                
        var incomingList = (incomingObject["items"] as JArray)!.ToObject<List<Dictionary<string, object>>>();

        foreach (var relationshipDataRow in relationshipDataRows)
        {
            var rightUri = (string) relationshipDataRow.Values["b.uri"];
            var (rightContentType, rightId, _) = GetContentTypeAndId(rightUri);

            if (incomingList.Any(incomingItem =>
                (string)incomingItem["contentType"] == rightContentType && (string) incomingItem["id"] == rightId.ToString()))
            {
                Console.WriteLine(
                    $"Link already exists for {contentType} ({innerCount} of {grouped.Count}) - overall {count}c of {total}.");

                innerCount++;
                continue;
            }

            incomingList.Add(new Dictionary<string, object>
            {
                {"contentType", rightContentType.ToLower()},
                {"id", rightId.ToString()}
            });
        }

        incomingObject["items"] = incomingList;
        curies[incomingPosition] = incomingObject;
        links["curies"] = curies;
        document["_links"] = links;
        
        Console.WriteLine($"Saving incoming relationship(s) for {contentType} ({innerCount} of {grouped.Count}) - overall {count}c of {total}.");
        await SaveToCosmosDb(document);
        
        innerCount++;
    }
}

async Task AddContHasLinks(string contentType)
{
    var downwardsRelationshipData = await RunQuery(new Query($"match (a:{contentType})-[r]->(b) return a.uri, r, b.uri"), neo4JUsername, true);
    Console.WriteLine($"{downwardsRelationshipData.Count} downward relationship records found for {contentType} - overall {count}b of {total}.");
    var innerCount = 1;

    var grouped = downwardsRelationshipData.GroupBy(x => (string) x.Values["a.uri"]).ToList();

    foreach (var relationshipDataRows in grouped)
    {
        var leftUri = relationshipDataRows.Key;
        var (_, id, _) = GetContentTypeAndId(leftUri);

        Console.WriteLine($"Fetching document for {contentType} {id} ({innerCount} of {grouped.Count}) - overall {count}b of {total}.");
        var document = await GetFromCosmosDb(id, contentType.ToLower());
        var links = (document["_links"] as JObject)!.ToObject<Dictionary<string, object>>();

        foreach (var relationshipDataRow in relationshipDataRows)
        {
            var rightUri = (string) relationshipDataRow.Values["b.uri"];
            var (rightContentType, _, contUrl) = GetContentTypeAndId(rightUri);
            var path = rightUri.Replace(contUrl, string.Empty);
            
            var toAdd = new Dictionary<string, object>
            {
                { "href", path },
                { "contentType", rightContentType }
            };

            var relationshipContentType = (relationshipDataRow.Values["r"] as IRelationship)!.Type;
            var key = $"cont:{relationshipContentType}";
            
            if (links.ContainsKey(key))
            {
                var contHas = links[key];

                switch (contHas)
                {
                    case JObject jo:
                    {
                        var djo = jo.ToObject<Dictionary<string, object>>();
                        var l = new List<Dictionary<string, object>>
                        {
                            djo,
                            toAdd
                        };

                        links[key] = l;
                        break;
                    }
                    case Dictionary<string, object> djo:
                    {
                        var l = new List<Dictionary<string, object>>
                        {
                            djo,
                            toAdd
                        };

                        links[key] = l;
                        break;
                    }
                    case JArray jaa:
                    {
                        var ja = jaa.ToObject<List<Dictionary<string, Object>>>();
                        ja.Add(toAdd);

                        links[key] = ja;
                        break;
                    }
                    case List<Dictionary<string, Object>> ja:
                        ja.Add(toAdd);
                        links[key] = ja;
                        break;
                    default:
                        throw new Exception($"Didn't expect type {contHas.GetType().Name}");
                }
            }
            else
            {
                links.Add(key, toAdd);
            }
        }

        document["_links"] = links;
        
        Console.WriteLine($"Saving cont:has relationship(s) ({relationshipDataRows.Key}) for {contentType} {id} ({innerCount} of {grouped.Count}) - overall {count}b of {total}.");
        await SaveToCosmosDb(document);
        
        innerCount++;
    }
}

async Task SaveDocumentsWithoutRelationships(string contentType)
{
    Console.WriteLine($"Requesting node data from {contentType}. Overal {count}a of {total}");
    
    var dataForLabel = await RunQuery(new Query($"match (a:{contentType}) return a"), neo4JUsername, true);
    Console.WriteLine($"{dataForLabel.Count} records found for {contentType} - overall {count}a of {total}.");
    var innerCount = 1;
    
    foreach (var dataRow in dataForLabel)
    {
        var node = dataRow.Values["a"] as INode;
        var uri = node!.Properties["uri"].As<string>();
        
        var (_, id, cont) = GetContentTypeAndId(uri);
        var curies = new List<Dictionary<string, object>>
        {
            new()
            {
                { "name", "cont" },
                { "href", cont }
            },
            new()
            {
                { "name", "incoming" },
                { "items", new List<Dictionary<string, object>>() }
            }
        };
        
        var links = new Dictionary<string, object>
        {
            { "self", uri },
            { "curies", curies }
        };
        
        var properties = new Dictionary<string, object>(node.Properties)
        {
            {"id", id.ToString()},
            {"ContentType", contentType.ToLower() },
            {"_links", links}
        };

        if (properties.ContainsKey("ModifiedDate") && properties["ModifiedDate"] is ZonedDateTime modifiedDate)
        {
            properties["ModifiedDate"] = modifiedDate.ToDateTimeOffset().UtcDateTime.ToString("o");
        }
        
        if (properties.ContainsKey("CreatedDate") && properties["CreatedDate"] is ZonedDateTime createdDate)
        {
            properties["CreatedDate"] = createdDate.ToDateTimeOffset().UtcDateTime.ToString("o");
        }
        
        Console.WriteLine($"Saving record {id} for {contentType} ({innerCount++} of {dataForLabel.Count}) - overall {count}a of {total}.");
        await SaveToCosmosDb(properties);
    }
}

static (string ContentType, Guid Id, string Cont) GetContentTypeAndId(string uri)
{
    try
    {
        var uriType = new Uri(uri, UriKind.Absolute);
        var pathOnly = uriType.AbsolutePath;
        pathOnly = pathOnly.ToLower().Replace("/api/execute", string.Empty);

        var uriParts = pathOnly.Trim('/').Split('/');
        var contentType = uriParts[0].ToLower();
        var id = Guid.Parse(uriParts[1]);

        return (contentType, id, $"{uriType.Scheme}://{uriType.Host}/api/execute");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        throw;
    }
}

async Task<Dictionary<string, object>> GetFromCosmosDb(Guid id, string contentType)
{
    var container = GetContainer();
    var tries = 0;

    while (tries++ < 5)
    {
        try
        {
            return await container.ReadItemAsync<Dictionary<string, object>>(id.ToString(), new PartitionKey(contentType));
        }
        catch
        {
            await Task.Delay(5000);
        }
    }

    throw new Exception("Failed to get after 5 retries");
}

async Task SaveToCosmosDb(Dictionary<string, object> properties)
{
    var container = GetContainer();
    var contentType = properties["ContentType"].As<string>();
    var tries = 0;
    
    while (tries++ < 5)
    {
        try
        {
            
            await container.UpsertItemAsync(properties, new PartitionKey(contentType));
            return;
        }
        catch
        {
            await Task.Delay(5000);
        }
    }

    throw new Exception("Failed to save after 5 retries");
}

Container GetContainer()
{
    return cosmosDb.GetDatabase(cosmosDbDatabaseName).GetContainer(cosmosDbContainerName);
}

async Task<List<IRecord>> RunQuery(Query query, string databaseName, bool defaultDatabase)
{
    var session = GetAsyncSession(databaseName, defaultDatabase);

    try
    {
        var cursor = await session.RunAsync(query);
        return await cursor.ToListAsync(record => record);
    }
    finally
    {
        await session.CloseAsync();
    }
}

IAsyncSession GetAsyncSession(string database, bool defaultDatabase)
{
    return defaultDatabase ? GetNeo4JDriver().AsyncSession()
        : GetNeo4JDriver().AsyncSession(builder => builder.WithDatabase(database));
}