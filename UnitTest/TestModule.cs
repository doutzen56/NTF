using NTF.Modules;

namespace UnitTest
{
    [DependsOn(typeof(NtfModule))]
    public class TestModule : NtfModule
    {
        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
