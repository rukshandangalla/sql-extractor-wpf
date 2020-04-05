using System;

namespace EclipseScriptGenerator.models
{
    public class SProc
    {
        public Boolean IsSelected { get; set; }

        public string ROUTINE_SCHEMA { get; set; }

        public string ROUTINE_NAME { get; set; }

        public string SPName { get; set; }

        public DateTime CREATED { get; set; }

        public DateTime LAST_ALTERED { get; set; }
    }
}
