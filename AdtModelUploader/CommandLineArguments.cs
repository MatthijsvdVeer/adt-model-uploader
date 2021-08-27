using CommandLine;

namespace AdtModelUploader
{
    internal sealed class CommandLineArguments
    {
        [Option('p', "path", Required = true, HelpText = "The path to the on-disk directory holding DTDL models.")]
        public string ModelPath { get; set; }

        [Option(
            'u',
            "url",
            Required = true,
            HelpText = "The URL for the Azure Digital Twin instance. https:// needs to be included.")]
        public string AdtUri { get; set; }

        [Option(
            't',
            "tenantId",
            Required = true,
            HelpText = "The tenant ID of the tenant for the Azure Digital Twin instance.")]
        public string TenantId { get; set; }

        [Option('c', "clientId", Required = true, HelpText = "The client ID as registered in the tenant.")]
        public string ClientId { get; set; }
    }
}