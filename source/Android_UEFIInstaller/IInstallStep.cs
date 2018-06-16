using System;

namespace Android_UEFIInstaller
{
    public interface IInstallStep
    {
        String GetDescription();
        bool Run(Object args = null);
        bool Revert(Object args = null);
    }
}
