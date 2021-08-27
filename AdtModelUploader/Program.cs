using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using CommandLine;
using Microsoft.Azure.DigitalTwins.Parser;

namespace AdtModelUploader
{
    internal class Program
    {
        private static DigitalTwinsRepository digitalTwinsRepository;

        private static string tenantId;

        private static string clientId;

        private static string instanceUrl;

        static async Task Main(string[] args)
        {
            var modelPath = string.Empty;

            Parser.Default.ParseArguments<CommandLineArguments>(args)
                .WithParsed(
                    o =>
                    {
                        modelPath = o.ModelPath;
                        clientId = o.ClientId;
                        tenantId = o.TenantId;
                        instanceUrl = o.AdtUri;
                    }
                );
            
            digitalTwinsRepository = new DigitalTwinsRepository(
                new Uri(instanceUrl),
                new InteractiveBrowserCredential(tenantId, clientId));

            var files = Directory.EnumerateFiles(modelPath, "*.json", SearchOption.AllDirectories);
            var modelDtdl = files.Select(File.ReadAllText);
            var modelParser = new ModelParser();
            var dictionary = await modelParser.ParseAsync(modelDtdl);

            var interfaces = dictionary.Values
                .Where(info => info.EntityKind == DTEntityKind.Interface)
                .Cast<DTInterfaceInfo>().ToList();

            var dtmiNodes = CreateDtmiNodes(interfaces);
            var modelUploadProcessor = new ModelUploadProcessor(digitalTwinsRepository);
            await modelUploadProcessor.UploadModelsBatched(dtmiNodes);
        }

        private static HashSet<DtmiNode> CreateDtmiNodes(List<DTInterfaceInfo> interfaces)
        {
            var dtmiNodes = new HashSet<DtmiNode>();
            foreach (var dtInterfaceInfo in interfaces)
            {
                var dtmiNode = DtmiNodeBuilder.CreateFor(dtInterfaceInfo)
                    .WithDependenciesFromComponents()
                    .WithDependenciesFromExtends()
                    .Build();
                dtmiNodes.Add(dtmiNode);
            }

            return dtmiNodes;
        }
    }
}