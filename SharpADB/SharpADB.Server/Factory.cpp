#include "pch.h"
#include "Factory.h"
#include "main.h"

using namespace SharpADB::Metadata;
using namespace std::chrono;

const CLSID& Factory::GetCLSID()
{
    static const CLSID CLSID_LoopUtil = { 0x88b86807, 0x897, 0x4061, { 0xac, 0x3a, 0x6f, 0x91, 0xf9, 0xed, 0x48, 0x96 } }; // 88B86807-0897-4061-AC3A-6F91F9ED4896
    return CLSID_LoopUtil;
}

Windows::Foundation::IAsyncAction Factory::ReleaseServerAsync()
{
    _conReleaseCount++;
    co_await resume_after(seconds(10));
    _conReleaseCount--;
    if (_conReleaseCount) { co_return; }
    _releaseNotifier();
}

Windows::Foundation::IInspectable Factory::ActivateInstance()
{
    ServerManager result = ServerManager::ServerManager();
    if (result == nullptr) { return nullptr; }
    CoAddRefServerProcess();
    result.ServerManagerDestructed(
        [](Windows::Foundation::IInspectable, bool)
        {
            if (CoReleaseServerProcess() == 0)
            {
                ReleaseServerAsync();
            }
        });
    return result.as<IInspectable>();
}

HRESULT STDMETHODCALLTYPE Factory::CreateInstance(::IUnknown* pUnkOuter, REFIID riid, void** ppvObject) try
{
    if (!ppvObject) { return E_POINTER; }
    *ppvObject = nullptr;
    if (pUnkOuter != nullptr) { return CLASS_E_NOAGGREGATION; }
    ServerManager result = ServerManager::ServerManager();
    if (!result) { return S_FALSE; }
    CoAddRefServerProcess();
    result.ServerManagerDestructed(
        [](Windows::Foundation::IInspectable, bool)
        {
            if (CoReleaseServerProcess() == 0)
            {
                ReleaseServerAsync();
            }
        });
    return result.as(riid, ppvObject);
}
catch (...)
{
    return to_hresult();
}

HRESULT STDMETHODCALLTYPE Factory::LockServer(BOOL fLock) try
{
    if (fLock)
    {
        CoAddRefServerProcess();
    }
    else if (CoReleaseServerProcess() == 0)
    {
        ReleaseServerAsync();
    }
    return S_OK;
}
catch (...)
{
    return to_hresult();
}
