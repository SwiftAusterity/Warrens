using System;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Details what keywords match a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IgnoreAutomatedBackupAttribute : Attribute
    {
    }
}
