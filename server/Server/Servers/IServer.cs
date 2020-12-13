using System.Threading;
using System.Threading.Tasks;

namespace Server.Servers
{
    interface IServer
    {
        void Run(CancellationToken cancellationToken);
    }
}
