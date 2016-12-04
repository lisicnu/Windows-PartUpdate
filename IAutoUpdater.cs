
using System;

namespace Eden.Update
{
    public interface IAutoUpdater
    {
        ExitCode Update();

        void RollBack();
    }
}
