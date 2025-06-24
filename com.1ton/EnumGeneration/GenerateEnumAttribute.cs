using System;

namespace OneTon.Utilities.ScriptableObjectHelpers.EnumGeneration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GenerateEnumAttribute : Attribute
    {
        public string EnumName { get; }
        public string OutputPath { get; }

        public GenerateEnumAttribute(string enumName = null, string outputPath = null)
        {
            EnumName = enumName;
            OutputPath = outputPath;
        }
    }
}