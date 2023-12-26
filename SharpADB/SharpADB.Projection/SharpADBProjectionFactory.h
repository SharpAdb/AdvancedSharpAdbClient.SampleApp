#pragma once

#include "SharpADBProjectionFactory.g.h"
#include "winrt/SharpADB.Metadata.h"

using namespace winrt::SharpADB::Metadata;

namespace winrt::SharpADB::Projection::implementation
{
    struct SharpADBProjectionFactory : SharpADBProjectionFactoryT<SharpADBProjectionFactory>
    {
        static ServerManager ServerManager();
    };
}

namespace winrt::SharpADB::Projection::factory_implementation
{
    struct SharpADBProjectionFactory : SharpADBProjectionFactoryT<SharpADBProjectionFactory, implementation::SharpADBProjectionFactory>
    {
    };
}
