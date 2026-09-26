using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UninstallTools;
using UninstallTools.Junk.Confidence;
using UninstallTools.Junk.Containers;

namespace BulkCrapUninstallerTests
{
    [TestClass]
    public class ConfidenceGeneratorsTests
    {
        [TestMethod]
        public void SameProductVersionStillInstalled_DemotesSharedJunkBelowGood()
        {
            var target = CreateBlenderEntry("Blender 4.4");
            var stillInstalled = CreateBlenderEntry("Blender 4.2");
            var junk = CreateBlenderSettingsJunk(target);

            ConfidenceGenerators.TestForSimilarNames(
                target,
                new[] { stillInstalled },
                new List<KeyValuePair<JunkResultBase, string>> { new(junk, "Blender") });

            Assert.IsTrue(
                junk.Confidence.GetConfidence() < ConfidenceLevel.Good,
                $"Shared settings remained {junk.Confidence.GetConfidence()} (raw {junk.Confidence.GetRawConfidence()})");
        }


        [TestMethod]
        public void NoSameProductRemains_DoesNotDemoteOwnedJunk()
        {
            var target = CreateBlenderEntry("Blender 4.4");
            var junk = CreateBlenderSettingsJunk(target);
            var initialConfidence = junk.Confidence.GetConfidence();

            ConfidenceGenerators.TestForSimilarNames(
                target,
                new ApplicationUninstallerEntry[0],
                new List<KeyValuePair<JunkResultBase, string>> { new(junk, "Blender") });

            Assert.AreEqual(initialConfidence, junk.Confidence.GetConfidence());
            Assert.AreEqual(ConfidenceLevel.Good, junk.Confidence.GetConfidence());
        }

        [TestMethod]
        public void UnrelatedProductInstalled_DoesNotDemoteOwnedJunk()
        {
            var target = CreateBlenderEntry("Blender 4.4");
            var unrelated = new ApplicationUninstallerEntry
            {
                RawDisplayName = "Krita 5.2",
                Publisher = "KDE"
            };
            var junk = CreateBlenderSettingsJunk(target);
            var initialConfidence = junk.Confidence.GetConfidence();

            ConfidenceGenerators.TestForSimilarNames(
                target,
                new[] { unrelated },
                new List<KeyValuePair<JunkResultBase, string>> { new(junk, "Blender") });

            Assert.AreEqual(initialConfidence, junk.Confidence.GetConfidence());
            Assert.AreEqual(ConfidenceLevel.Good, junk.Confidence.GetConfidence());
        }

        private static ApplicationUninstallerEntry CreateBlenderEntry(string displayName)
        {
            return new ApplicationUninstallerEntry
            {
                RawDisplayName = displayName,
                Publisher = "Blender Foundation"
            };
        }

        private static TestJunkResult CreateBlenderSettingsJunk(ApplicationUninstallerEntry target)
        {
            var junk = new TestJunkResult(target);
            junk.Confidence.AddRange(ConfidenceGenerators.GenerateConfidence(
                "Blender",
                @"C:\Users\test\AppData\Roaming\Blender Foundation",
                1,
                target));
            return junk;
        }

        private sealed class TestJunkResult : JunkResultBase
        {
            public TestJunkResult(ApplicationUninstallerEntry application) : base(application, null)
            {
            }

            public override void Backup(string backupDirectory) { }
            public override void Delete() { }
            public override void Open() { }
            public override string GetDisplayName() => "Blender";
        }
    }
}
