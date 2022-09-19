using NewLife;
using NewLife.Net;
using NewLife.Net.Proxy;

namespace XProxy.Proxy;

public class NATSession : ProxySession
{
    Int32 _prefix = 0;
    protected override void OnReceive(ReceivedEventArgs e)
    {
        var pk = e.Packet;
        if (_prefix == 0 && pk.Total > 0)
        {
            var pf = (Host as NATProxy).Prefix;
            if (!pf.IsNullOrEmpty())
            {
                if (pk.Slice(0, pf.Length / 2).ToHex() == pf)
                    _prefix = 1;
                else
                    _prefix = 2;
            }
            else
                _prefix = 1;
        }

        if (_prefix == 2) return;

        base.OnReceive(e);
    }
}