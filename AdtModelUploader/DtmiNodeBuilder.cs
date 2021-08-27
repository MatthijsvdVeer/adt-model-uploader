using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Azure.DigitalTwins.Parser;

namespace AdtModelUploader
{
    internal sealed class DtmiNodeBuilder
    {
        private readonly DTInterfaceInfo dtInterfaceInfo;

        private readonly Collection<Action<DtmiNode>> builderFunctions;

        private DtmiNodeBuilder(DTInterfaceInfo dtInterfaceInfo)
        {
            this.dtInterfaceInfo = dtInterfaceInfo;
            this.builderFunctions = new Collection<Action<DtmiNode>>();
        }

        public static DtmiNodeBuilder CreateFor(DTInterfaceInfo dtInterfaceInfo)
        {
            return new(dtInterfaceInfo);
        }

        public DtmiNodeBuilder WithDependenciesFromExtends()
        {
            this.builderFunctions.Add(node =>
            {
                var extends = this.dtInterfaceInfo.Extends.Select(info => info.Id);
                foreach (var dtmi in extends)
                {
                    node.DependsOn.Add(dtmi);
                }
            });

            return this;
        }

        public DtmiNodeBuilder WithDependenciesFromComponents()
        {
            this.builderFunctions.Add(node =>
            {
                var components = this.dtInterfaceInfo.Contents.Values
                    .Where(contentInfo => contentInfo.EntityKind == DTEntityKind.Component)
                    .Cast<DTComponentInfo>()
                    .Select(info => info.Schema.Id);
                foreach (var dtmi in components)
                {
                    node.DependsOn.Add(dtmi);
                }
            });

            return this;
        }

        public DtmiNode Build()
        {
            var node = new DtmiNode(this.dtInterfaceInfo);
            foreach (var builderFunction in this.builderFunctions)
            {
                builderFunction.Invoke(node);
            }

            return node;
        }
    }
}