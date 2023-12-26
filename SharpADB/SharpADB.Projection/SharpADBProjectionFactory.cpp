#include "pch.h"
#include "SharpADBProjectionFactory.h"
#include "SharpADBProjectionFactory.g.cpp"

namespace winrt::SharpADB::Projection::implementation
{
    static const CLSID CLSID_ServerManager = { 0x88b86807, 0x897, 0x4061, { 0xac, 0x3a, 0x6f, 0x91, 0xf9, 0xed, 0x48, 0x96 } }; // 88B86807-0897-4061-AC3A-6F91F9ED4896

    ServerManager SharpADBProjectionFactory::ServerManager()
    {
        return try_create_instance<::ServerManager>(CLSID_ServerManager, CLSCTX_ALL);
    }
}
