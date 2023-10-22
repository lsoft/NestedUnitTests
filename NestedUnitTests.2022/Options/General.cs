using Community.VisualStudio.Toolkit;
using System.ComponentModel;

namespace NestedUnitTests
{
    internal partial class OptionsProvider
    {
        public class GeneralOptions : BaseOptionPage<General>
        {
        }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("General")]
        [DisplayName("File name suffix")]
        [Description("Suffix for unit test source files (e.g. Fixture, UnitTests).")]
        [DefaultValue("Fixture")]
        public string FileNameSuffix { get; set; } = "Fixture";

        [Category("General")]
        [DisplayName("Conditional symbol")]
        [Description("Conditional symbol to exclude nested unit test from compilation. Use this conditional symbol for production-ready builds.")]
        [DefaultValue("SKIP_NESTED_TESTS")]
        public string ConditionalSymbol { get; set; } = "SKIP_NESTED_TESTS";

        [Category("General")]
        [DisplayName("Open new file")]
        [Description("Do you want to open a new unit tests file automatically?")]
        [DefaultValue(true)]
        public bool OpenNewFile { get; set; } = true;
    }
}
