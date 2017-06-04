using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace USBMonitor
{
    [AttributeUsageAttribute(AttributeTargets.Assembly, Inherited = false)]
    [ComVisibleAttribute(true)]
    public sealed class ImportedFromTypeLibAttribute : Attribute
    {

    }
}
