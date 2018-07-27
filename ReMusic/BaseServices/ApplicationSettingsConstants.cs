using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMusic.BaseServices
{
    public class ApplicationSettingsConstants
    {
        public const string Volume = "volume";
        public const string Position = "position";
        public const string BackgroundTaskState = "backgroundtaskstate"; // Started, Running, Cancelled
        public const string AppState = "appstate"; // Suspended, Resumed
        public const string AppSuspendedTimestamp = "appsuspendedtimestamp";
        public const string AppResumedTimestamp = "appresumedtimestamp";
    }
}
