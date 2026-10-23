using VyshKat.Core.Model;

namespace VyshKat.Core.Services;

public sealed class ServiceCenter : IServiceCenter
{
    public bool Inspect(Transport transport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        return transport.IsTechnicallySound;
    }
}
