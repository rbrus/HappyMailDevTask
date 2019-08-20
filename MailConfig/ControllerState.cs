using System;
using System.Collections.Generic;
using System.Text;

namespace Config
{
    /// <summary>
    /// Determines a controller state.
    /// </summary>
    public enum ControllerState
    {
        Connecting,
        Connected,
        Disconnected,
        Reconnecting,
        LoggingIn,
        Error
    }
}
