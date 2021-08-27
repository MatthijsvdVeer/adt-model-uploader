using System.Collections.Generic;
using Microsoft.Azure.DigitalTwins.Parser;

namespace AdtModelUploader
{
    internal sealed class DtmiNode
    {
        public DTInterfaceInfo InterfaceInfo { get; }

        public HashSet<Dtmi> DependsOn { get; } = new();

        public DtmiNode(DTInterfaceInfo interfaceInfo) => this.InterfaceInfo = interfaceInfo;

        public override int GetHashCode() => this.InterfaceInfo.Id.GetHashCode();
    }
}