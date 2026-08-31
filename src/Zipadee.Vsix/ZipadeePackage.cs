using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace Zipadee.Vsix
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    public sealed class ZipadeePackage : AsyncPackage
    {
        public const string PackageGuidString = "e0e0d446-dd6d-4aac-8bbd-13debf9a4c62";

        protected override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            return Task.CompletedTask;
        }
    }
}
