using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AdtModelUploader
{
    internal sealed class ModelUploadProcessor
    {
        private readonly DigitalTwinsRepository digitalTwinsRepository;

        public ModelUploadProcessor(DigitalTwinsRepository digitalTwinsRepository)
        {
            this.digitalTwinsRepository = digitalTwinsRepository;
        }

        public async Task UploadModelsBatched(HashSet<DtmiNode> dtmiNodes)
        {
            var iterations = 0;
            while (dtmiNodes.Count > 0)
            {
                iterations++;
                Console.WriteLine($"Starting iteration {iterations}, {dtmiNodes.Count} interfaces remaining.");

                var withoutDependency = dtmiNodes.Where(node => node.DependsOn.Count == 0).ToList();
                if (withoutDependency.Count == 0)
                {
                    withoutDependency.AddRange(this.FindSimpleCircularDependencies(dtmiNodes));
                }

                if (withoutDependency.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Can't resolve state, no models without dependencies or simple circular dependencies found.");
                }

                Console.WriteLine($"Uploading {withoutDependency.Count} models.");
                await this.UploadTwinModels(
                    withoutDependency.Select(node => node.InterfaceInfo.GetDtdl(false, false)).ToList());
                foreach (var dtmiNode in withoutDependency)
                {
                    dtmiNodes.Remove(dtmiNode);
                }

                foreach (var node in dtmiNodes)
                {
                    node.DependsOn.RemoveWhere(
                        dtmi => withoutDependency.Any(dtmiNode => dtmiNode.InterfaceInfo.Id == dtmi));
                }

                Console.WriteLine($"Iteration {iterations} completed.");
            }
        }

        /// <summary>
        /// Returns the first pair of models that depend on each other.
        /// </summary>
        private IEnumerable<DtmiNode> FindSimpleCircularDependencies(HashSet<DtmiNode> dtmiNodes)
        {
            var nodes = new List<DtmiNode>();
            foreach (var sourceNode in dtmiNodes)
            {
                if (sourceNode.DependsOn.Count != 1)
                {
                    continue;
                }

                var targetNodes =
                    (from targetNode in dtmiNodes
                     where targetNode.DependsOn.Count == 1
                     where targetNode.InterfaceInfo.Id != sourceNode.InterfaceInfo.Id
                     where sourceNode.InterfaceInfo.Id == targetNode.DependsOn.Single()
                           && targetNode.InterfaceInfo.Id == sourceNode.DependsOn.Single()
                     select targetNode).ToList();

                if (!targetNodes.Any())
                {
                    continue;
                }

                nodes.AddRange(new[] { sourceNode, targetNodes.Single() });
                return nodes;
            }

            return nodes;
        }

        private async Task UploadTwinModels(List<string> models)
        {
            await this.digitalTwinsRepository.CreateModelsAsync(models, CancellationToken.None);
        }
    }
}
