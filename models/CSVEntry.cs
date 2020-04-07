using System;

namespace EclipseScriptGenerator.models
{
    public class CSVEntry
    {
        public string StoryId { get; set; }

        public string ScriptName { get; set; }

        public string ScriptType { get; set; }

        public string ScriptRisk { get; set; }

        public string DB { get; set; }

        public string CreatedBy { get; set; }

        public string QAReleaseDate { get; set; }

        public string ReleaseNumber { get; set; }

        public string ScriptLink { get; set; }

        public string Tag { get; set; }

        public string Status { get; set; }
    }
}
