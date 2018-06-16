using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    public enum InstallationOperation
    {
        SYS_INSTALL,
        SYS_REMOVE,
        SYS_UPDATE
    }


    enum UI_STATUS
    {
        ENABLE,
        DISABLE,
        OFF,
        READY
    } ;

    struct TaskInfo
    {
        public String path;
        public String drive;
        public String size;
        public InstallationOperation operation;
    }
}
