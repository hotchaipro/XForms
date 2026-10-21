using System;

namespace XForms.Platform
{
    /// <summary>
    /// Marks an internal member of a framework type as part of the platform
    /// contract, so platform code may use it. Everything else a platform uses
    /// of the framework's internals is in this namespace; the XF0001 analyzer
    /// reports any other use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Constructor | AttributeTargets.Field)]
    internal sealed class PlatformContractAttribute : Attribute
    {
    }
}
