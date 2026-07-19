using System.Text.Json.Nodes;

namespace SsalddelApp.Services.Commerce;

public sealed record CommerceChannelListingPreparation(
    CommerceChannelDescriptor Channel,
    JsonNode? PayloadDraft,
    string SyncStatus,
    string Message);
