using System;
using System.Collections.Generic;
using System.Text;
using TelePush.Backend.Core;

namespace TelePush.Backend.Attribute
{
    class CommandAttribute : System.Attribute
    {
        //Controller method with command attribute takes one extra nullable parameter.
        public CommandAttribute(string commandName)
        {
            CommandName = commandName;
        }

        public string CommandName { get; set; }
    }

    class TypeAttribute : System.Attribute
    {
        public TypeAttribute(DispatcherType type)
        {
            Type = type;
        }

        public DispatcherType Type { get; set; }
    }
}
