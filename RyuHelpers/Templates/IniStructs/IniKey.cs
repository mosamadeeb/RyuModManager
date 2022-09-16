using System.Collections.Generic;

namespace RyuHelpers.Templates
{
    public struct IniKey
    {
        public string Name { get; set; }

        public List<string> Comments { get; set; }

        public int DefaultValue { get; set; }
    }
}
