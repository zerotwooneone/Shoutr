using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Shoutr")]
namespace Shoutr.Contracts
{
    internal class Defaults
    {
        public const int Port = 3036;
        public const int Mtu = 1400;
        public const int MinPort = 1025;
        public const int MaxPort = 65535;
    }
}