using System;
using System.Collections.Generic;

namespace qASIC.Console.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class CommandTargetAttribute : Attribute
    {
        public abstract List<object> GetTargets(Type targetType);
    }
}