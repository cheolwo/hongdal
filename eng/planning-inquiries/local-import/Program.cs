using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;
using Ssalddel.Services.Content;

// Explicit local maintenance entry, not an HTTP endpoint, login, background worker, or generic SQL runner.
var result = new Dictionary<string, object?> { ["http"] = "NotUsed", ["databaseWriteAttempted"] = false,
    ["newDefinitions"] = 0, ["assetSelections"] = 0, ["worldInstances"] = 0 };
string? output = null;
var ownsOutput = false;
try
{
    Guard(args.Length == 6, "Usage: preview|apply repository packetRef packetSha256 outputRef approvalSha256");
    var mode = args[0]; Guard(mode is "preview" or "apply", "ModeInvalid");
    var repo = Path.GetFullPath(args[1]);
    var packetPath = Safe(repo, args[2], true);
    output = Safe(repo, args[4], true);
    Guard(!Directory.Exists(output), "OutputAlreadyExists");
    // This local operator can only consume the bounded approval in this repository.
    var approval = Safe(repo, "docs/Architecture/기획판본-서버반입파이프라인.md", false);
    var packetBytes = File.ReadAllBytes(packetPath);
    Guard(packetBytes.Length is > 0 and <= 4 * 1024 * 1024 && HashBytes(packetBytes) == args[3], "PacketHashMismatch");
    using var doc = JsonDocument.Parse(packetBytes);
    var p = doc.RootElement;
    var visualImport = p.GetProperty("localImport").GetProperty("scope").GetString() == "ExistingDefinitions_AutomaticVisualDraftOnly_WiRelationsNotImported";
    if (visualImport) approval = Safe(repo, "docs/AI/개체자산-자동할당-D440-개발인계-2026-08-31.md", false);
    Guard(Hash(approval) == args[5], "ApprovalDrift");
    Guard(p.GetProperty("schemaVersion").GetString() == "planning-handoff.v1" &&
        p.GetProperty("state").GetString() == "Prepared_NotSubmitted" &&
        p.GetProperty("localImport").GetProperty("state").GetString() == "Prepared_NotApplied", "PacketNotImportable");
    var sourceRef = p.GetProperty("sourceRef").GetString()!;
    var packetFolder = args[2][..args[2].LastIndexOf('/')];
    // Rebuild the packet from the current document; do not trust an edited packet's request or dependencies.
    var check = new ProcessStartInfo("pwsh") { UseShellExecute = false, CreateNoWindow = true,
        RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var a in new[] { "-NoProfile", "-File", Path.Combine(repo,"eng/planning-inquiries/manage-planning-release.ps1"),
        "-Mode", "Check", "-DocumentPath", sourceRef, "-OutputDirectory", packetFolder }) check.ArgumentList.Add(a);
    check.WorkingDirectory = repo;
    using (var process = Process.Start(check)!)
    {
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        await process.WaitForExitAsync(timeout.Token); await stderr;
        Guard(process.ExitCode == 0, "DocumentValidationFailed");
        using var validation = JsonDocument.Parse(await stdout);
        Guard(validation.RootElement.GetProperty("packetSha256").GetString() == args[3] &&
            validation.RootElement.GetProperty("outputRef").GetString() == args[2], "RebuiltPacketMismatch");
    }
    var request = p.GetProperty("localImport").GetProperty("request").Deserialize<게임객체WI추출Request>()!;
    Guard(request.Definitions.Count == 0 && request.Relations.Count is > 0 and <= 64 &&
        request.Relations.All(x => x.DefinitionId is not null && x.ExtractionState == "ExistingDefinitionReuse"), "ExistingDefinitionsOnly");
    void Fresh()
    {
        Guard(Hash(packetPath) == args[3] && Hash(approval) == args[5], "InputDrift");
        foreach (var dependency in p.GetProperty("dependencies").EnumerateArray())
            Guard(Hash(Safe(repo,dependency.GetProperty("path").GetString()!,false)) == dependency.GetProperty("sha256").GetString(), "DependencyDrift");
    }
    Fresh();
    using var docker = await InspectDocker();
    var container = docker.RootElement[0];
    Guard(container.GetProperty("Name").GetString() == "/hongdal-mysql-1" &&
        container.GetProperty("State").GetProperty("Running").GetBoolean(), "LocalContainerUnavailable");
    var labels = container.GetProperty("Config").GetProperty("Labels");
    Guard(labels.GetProperty("com.docker.compose.project").GetString() == "hongdal" &&
        labels.GetProperty("com.docker.compose.project.config_files").GetString()!.EndsWith("docker-compose.dev-deps.yml",StringComparison.Ordinal), "ComposeMismatch");
    var env = container.GetProperty("Config").GetProperty("Env").EnumerateArray()
        .Select(x => x.GetString()!.Split('=',2)).ToDictionary(x => x[0],x => x[1]);
    Guard(env["MYSQL_DATABASE"] == "hongdal_dev" && env["MYSQL_USER"] != "root", "DatabaseBoundary");
    Guard(container.GetProperty("NetworkSettings").GetProperty("Ports").GetProperty("3306/tcp")
        .EnumerateArray().Any(x => x.GetProperty("HostPort").GetString() == "13306"), "LocalPortMismatch");
    var cs = new MySqlConnectionStringBuilder { Server="127.0.0.1",Port=13306,Database="hongdal_dev",
        UserID=env["MYSQL_USER"],Password=env["MYSQL_PASSWORD"],PersistSecurityInfo=false,
        Pooling=false,ConnectionTimeout=10,DefaultCommandTimeout=20 };
    // Password lives in memory only. Exceptions/connection strings/raw docker output are never emitted.
    var dbOptions = new DbContextOptionsBuilder<개체시각대응DbContext>()
        .UseMySql(cs.ConnectionString,new MySqlServerVersion(new Version(8,4,0))).Options;
    var actor = new LocalOperator(visualImport);
    var principal = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor.UserId!),
        new(ClaimTypes.Role,actor.Role!)],"ExplicitLocalPlanningMaintenance"));
    var services = new ServiceCollection(); services.AddLogging();
    services.AddAuthorizationCore(o => o.AddPolicy(개체시각대응Codes.Policy,
        b => b.RequireAuthenticatedUser().RequireRole(살뜰.Data.역할명.서버관리자)));
    await using var provider = services.BuildServiceProvider();
    var auth = provider.GetRequiredService<IAuthorizationService>();
    var options = new FixedOptions(new() { ReviewEnabled=true,Enabled=false,EvidenceRoot=repo,
        UnitySourceRoot=visualImport ? @"C:\Users\user\ssalddel" : null });
    await using var db = new 개체시각대응DbContext(dbOptions);
    var definitions = new 게임객체시각구성UseCase(db,auth,actor,options,TimeProvider.System);
    var importer = new 게임객체WI참여UseCase(db,definitions,auth,actor,options,TimeProvider.System);
    if (visualImport)
    {
        var visuals = p.GetProperty("localImport").GetProperty("visuals").EnumerateArray().ToArray();
        Guard(visuals.Length is > 0 and <= 10, "VisualRoleLimit");
        var objectIds = p.GetProperty("decisions").GetProperty("objects").EnumerateArray()
            .ToDictionary(x => x.GetProperty("key").GetString()!, x => x.GetProperty("definitionId").GetString()!, StringComparer.Ordinal);
        Directory.CreateDirectory(output); ownsOutput = true;
        WriteNew(Path.Combine(output,"claim.json"),new { mode,packetSha256=args[3],scope="D440_AutomaticDraft_Max10_NoWiWrite_NoDdl" });
        result["operator"]=actor.UserId; result["target"]="hongdal-mysql-1 / hongdal_dev";
        result["wiRelations"]="NotImported_VisualOnly";
        var outcomes = new List<object>(); result["visualResults"]=outcomes; var failed=false; var inserted=0;
        foreach (var group in visuals.GroupBy(x=>objectIds[x.GetProperty("objectKey").GetString()!]))
        {
            Fresh();
            var expected=group.First().GetProperty("expectedRevision").GetInt64();
            Guard(group.All(x=>x.GetProperty("expectedRevision").GetInt64()==expected),"VisualRevisionConflict");
            var basis=await definitions.GetAsync(principal,group.Key,expected,default);
            if(basis.Diagnostic!="Found") { outcomes.Add(new {definitionId=group.Key,diagnostic=basis.Diagnostic});failed=true;continue; }
            var selected=group.Where(x=>x.GetProperty("state").GetString()=="AutomaticDraft").ToArray();
            foreach(var held in group.Except(selected)) outcomes.Add(new {definitionId=group.Key,role=held.GetProperty("role").GetString(),
                state=held.GetProperty("state").GetString(),reason=held.GetProperty("reason").GetString(),stored=false});
            if(selected.Length==0)continue;
            var items=basis.Composition!.Definition.Items.ToList();
            foreach(var v in selected)
            {
                var item=new 게임객체시각항목Input(v.GetProperty("key").GetString()!,v.GetProperty("role").GetString()!,v.GetProperty("slotKey").GetString()!,
                    InventorySnapshotId:v.GetProperty("inventorySnapshotId").GetString(),SelectionEvidenceJson:v.GetProperty("selectionEvidence").GetRawText());
                var previous=items.FindIndex(x=>x.ItemId==item.ItemId);
                if(previous>=0) { item=item with {AnchorIntent=items[previous].AnchorIntent}; items[previous]=item; } else items.Add(item);
            }
            var save=new 게임객체시각구성Request("planning-visual:"+args[3],expected,basis.Composition.Definition with {Items=items.ToArray()});
            if(mode=="preview") { outcomes.Add(new {definitionId=group.Key,status="Previewed_NotApplied",roles=selected.Length});continue; }
            result["databaseWriteAttempted"]=true;
            var first=await definitions.SaveAsync(principal,save,default);
            outcomes.Add(new {definitionId=group.Key,first});
            if(first.Diagnostic!="Persisted") { failed=true;continue; }
            if(!first.Duplicate)inserted+=selected.Length;
            WriteNew(Path.Combine(output,"composition-"+first.Composition!.CompositionId+".json"),first);
            var repeat=await definitions.SaveAsync(principal,save,default);
            Guard(repeat.Diagnostic=="Persisted"&&repeat.Duplicate,"VisualRepeatNotIdempotent");
            await using var independent=new 개체시각대응DbContext(dbOptions);
            var reader=new 게임객체시각구성UseCase(independent,auth,actor,options,TimeProvider.System);
            var observed=await reader.GetAsync(principal,group.Key,first.Composition.Revision,default);
            Guard(개체시각선택Policy.Hash(observed.Composition!)==개체시각선택Policy.Hash(first.Composition),"VisualIndependentMismatch");
            outcomes.Add(new {definitionId=group.Key,duplicate=true,independentMatch=true});
        }
        result["assetSelections"]=inserted;
        result["status"]=failed?"PartialOrBlocked_NotApplied":mode=="preview"?"Previewed_NotApplied":inserted==0?"NoNewSelections_HeldOrReplay":"DraftPersistedAndRequeried_NotApplied";
        if(failed)Environment.ExitCode=1;
    }
    else
    {
    var definitionIds = request.Relations.Select(x => x.DefinitionId!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    var before = new Dictionary<string,string>();
    foreach (var id in definitionIds)
    {
        var found = await definitions.GetAsync(principal,id,null,default);
        Guard(found.Diagnostic == "Found", "ExistingDefinitionNotFound");
        before[id] = 개체시각선택Policy.Hash(found.Composition!);
    }
    var useIds = request.Relations.Select(게임객체WI참여UseCase.Id).ToArray();
    // Keep the database expression on Enumerable, not the runtime's array/span extension overload.
    var oldRelations = await db.WiUses.AsNoTracking().Where(x => x.SourceHash == request.SourceHash && Enumerable.Contains(useIds,x.UseId)).ToArrayAsync();
    foreach (var old in oldRelations)
        Guard(old.InputHash == 개체시각선택Policy.Hash(request.Relations.Single(x => 게임객체WI참여UseCase.Id(x) == old.UseId)), "ExistingRelationConflict");
    result["target"] = "hongdal-mysql-1 / 127.0.0.1:13306 / hongdal_dev";
    result["operator"] = actor.UserId; result["packetSha256"] = args[3]; result["sourceRef"] = sourceRef;
    result["relationsPlanned"] = request.Relations.Count; result["relationsAlreadyPresent"] = oldRelations.Length;
    result["definitionsReused"] = definitionIds.Length;
    Directory.CreateDirectory(output);
    WriteNew(Path.Combine(output,"claim.json"),new { mode, packetSha256=args[3],scope="ExistingWiRelationsOnly_NoDdl_NoDefinitionsOrAssets" });
    ownsOutput = true;
    Fresh();
    if (mode == "preview") result["status"] = "Previewed_NotApplied";
    else
    {
        result["databaseWriteAttempted"] = true;
        var first = await importer.ImportAsync(principal,request,default); result["first"] = first;
        Guard(first.Diagnostic == "Persisted" && first.DefinitionsInserted == 0,"ImportNotPersisted");
        Fresh();
        var replay = await importer.ImportAsync(principal,request,default); result["repeat"] = replay;
        Guard(replay.Diagnostic == "Persisted" && replay.Duplicate && replay.RelationsInserted == 0,"RepeatNotIdempotent");
        await using var fresh = new 개체시각대응DbContext(dbOptions);
        var rows = await fresh.WiUses.AsNoTracking().Where(x => x.SourceHash == request.SourceHash && Enumerable.Contains(useIds,x.UseId)).ToArrayAsync();
        Guard(rows.Length == request.Relations.Count,"IndependentRowCountMismatch");
        foreach(var row in rows)
        {
            var expected = request.Relations.Single(x => 게임객체WI참여UseCase.Id(x) == row.UseId);
            Guard(row.InputHash == 개체시각선택Policy.Hash(expected) && row.DefinitionId == expected.DefinitionId &&
                row.WorldInteractionId == expected.WorldInteractionId && row.Role == expected.Role &&
                개체시각선택Policy.Hash(JsonSerializer.Deserialize<게임객체WI참여Input>(row.InputJson)!) == row.InputHash,"IndependentRelationMismatch");
            Guard(await fresh.Compositions.AnyAsync(x => x.CompositionId == row.DefinitionCompositionId && x.DefinitionId == row.DefinitionId),"CompositionReferenceMissing");
        }
        var freshDefinitions = new 게임객체시각구성UseCase(fresh,auth,actor,options,TimeProvider.System);
        foreach(var id in definitionIds)
            Guard(before[id] == 개체시각선택Policy.Hash((await freshDefinitions.GetAsync(principal,id,null,default)).Composition!),"ExistingDefinitionChanged");
        result["independentRowsMatched"] = rows.Length; result["existingDefinitionsUnchanged"] = true;
        result["status"] = "PersistedAndRequeried_NotGameApplied";
    }
    }
}
catch (Exception error)
{
    result["status"] = "Failed";
    result["reason"] = error is GateFailure ? error.Message : error.GetType().Name;
    result["innerExceptionType"] = error.InnerException?.GetType().Name;
    result["errorSite"] = new StackTrace(error,true).GetFrames().Where(x => x.GetFileLineNumber()>0).Take(8)
        .Select(x => new { method=x.GetMethod()?.Name, line=x.GetFileLineNumber() }).ToArray();
    Environment.ExitCode = 1;
}
finally
{
    if(ownsOutput && output is not null && !File.Exists(Path.Combine(output,"result.json")))
        WriteNew(Path.Combine(output,"result.json"),result);
    Console.WriteLine(JsonSerializer.Serialize(result,new JsonSerializerOptions { WriteIndented=true }));
}
static void Guard(bool passed,string reason) { if(!passed) throw new GateFailure(reason); }
static string HashBytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
static string Hash(string path) => HashBytes(File.ReadAllBytes(path));
static string Safe(string root,string relative,bool artifact)
{
    Guard(!string.IsNullOrWhiteSpace(relative) && !Path.IsPathRooted(relative) && !relative.Contains('\\') && !relative.Contains(':') &&
        !relative.Split('/').Any(x => x is "." or ".." or ""),"UnsafePath");
    Guard(artifact ? relative.StartsWith("artifacts/local/",StringComparison.Ordinal) :
        new[] {"docs/","eng/","artifacts/local/"}.Any(x => relative.StartsWith(x,StringComparison.Ordinal)),"PathOutsideScope");
    var full = Path.GetFullPath(Path.Combine(root,relative));
    Guard(full.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase),"PathEscape");
    for(string? check=full; check is not null; check=Path.GetDirectoryName(check))
        if(File.Exists(check) || Directory.Exists(check)) Guard((File.GetAttributes(check)&FileAttributes.ReparsePoint)==0,"ReparsePoint");
    return full;
}
static void WriteNew(string path,object value)
{
    using var file = new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None);
    using var writer = new StreamWriter(file,new UTF8Encoding(false));
    writer.Write(JsonSerializer.Serialize(value,new JsonSerializerOptions {WriteIndented=true}));
}
static async Task<JsonDocument> InspectDocker()
{
    var start = new ProcessStartInfo("docker") {UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
    start.ArgumentList.Add("inspect"); start.ArgumentList.Add("hongdal-mysql-1");
    using var p = Process.Start(start)!; var output=p.StandardOutput.ReadToEndAsync(); var error=p.StandardError.ReadToEndAsync();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    await p.WaitForExitAsync(timeout.Token); await error; Guard(p.ExitCode==0,"DockerUnavailable");
    return JsonDocument.Parse(await output);
}
sealed class GateFailure(string message) : Exception(message);
sealed class LocalOperator(bool visual = false) : ICurrentUserAccessor
{ public string? UserId => visual ? "local-maintenance:visual-d440" : "local-maintenance:planning-d439"; public string? Role => 살뜰.Data.역할명.서버관리자; }
sealed class FixedOptions(개체시각자산Options value) : IOptionsMonitor<개체시각자산Options>
{ public 개체시각자산Options CurrentValue => value; public 개체시각자산Options Get(string? name)=>value;
  public IDisposable? OnChange(Action<개체시각자산Options,string?> listener)=>null; }
