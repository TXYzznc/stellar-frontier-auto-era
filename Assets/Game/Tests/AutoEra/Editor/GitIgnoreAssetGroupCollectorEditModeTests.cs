using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class GitIgnoreAssetGroupCollectorEditModeTests
    {
        [Test]
        public void RootPatternsAndDirectoryRules_IgnoreMatchingAssets()
        {
            string[] rules = { "/Assets/Game/Models/", "*.unitypackage" };

            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Models/Vehicle.fbx", rules), Is.True);
            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Export/Delivery.unitypackage", rules), Is.True);
            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Scripts/Vehicle.cs", rules), Is.False);
        }

        [Test]
        public void Negation_LastMatchingRuleRestoresAPath()
        {
            string[] rules = { "Assets/Game/Textures/", "!Assets/Game/Textures/UI/Keep.png" };

            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Textures/UI/Drop.png", rules), Is.True);
            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Textures/UI/Keep.png", rules), Is.False);
        }

        [Test]
        public void DoubleStar_MatchesNestedPaths()
        {
            string[] rules = { "Assets/**/[Tt]emp/*.asset" };

            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Runtime/Temp/Generated.asset", rules), Is.True);
            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Runtime/Stable/Generated.asset", rules), Is.False);
        }

        [Test]
        public void NegatedCharacterClass_MatchesOnlyNonExcludedInitials()
        {
            string[] rules = { "Assets/Game/[!S]cratch/*.asset" };

            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Ccratch/Generated.asset", rules), Is.True);
            Assert.That(GitIgnoreAssetGroupCollector.IsIgnoredByGitIgnore("Assets/Game/Scratch/Generated.asset", rules), Is.False);
        }
    }
}
