using NUnit.Framework;

namespace Metaverso.Tests
{
    public class CoreRulesTests
    {
        [Test]
        public void NextPatch_IncrementsPatch()
        {
            Assert.AreEqual("0.1.1", BuildInfo.NextPatch("0.1.0"));
            Assert.AreEqual("1.2.10", BuildInfo.NextPatch("1.2.9"));
        }

        [Test]
        public void NextPatch_RejectsGarbage()
        {
            Assert.AreEqual("0.1.1", BuildInfo.NextPatch("beta"));
            Assert.AreEqual("0.1.1", BuildInfo.NextPatch(""));
        }

        [Test]
        public void RoomQuery_ReadsRoomAndName()
        {
            const string url = "https://juliojulioso.github.io/metaverso-mvp/?room=Cliente-A&name=Ana%20Ruiz";
            Assert.AreEqual("cliente-a", RoomQuery.FromUrl(url));
            Assert.AreEqual("Ana Ruiz", RoomQuery.DisplayNameFromUrl(url));
        }

        [Test]
        public void RoomQuery_FallsBackWhenMissing()
        {
            Assert.AreEqual(RoomQuery.DefaultRoom, RoomQuery.FromUrl("https://example.com/"));
            Assert.AreEqual(RoomQuery.DefaultRoom, RoomQuery.FromUrl(null));
            Assert.AreEqual(RoomQuery.DefaultName, RoomQuery.DisplayNameFromUrl(""));
        }

        [Test]
        public void RoomQuery_StripsUnsafeCharacters()
        {
            Assert.AreEqual("sala_1", RoomQuery.SanitizeRoom("Sala 1!"));
            Assert.AreEqual(RoomQuery.DefaultRoom, RoomQuery.SanitizeRoom("!!!"));
        }

        [Test]
        public void ArchRules_OnlyMatchTheArchFolder()
        {
            Assert.IsTrue(ArchImportRules.AppliesTo("Assets/_Metaverso/Models/Arch/edificio.fbx"));
            Assert.IsFalse(ArchImportRules.AppliesTo("Assets/_Metaverso/Models/prop.fbx"));
            Assert.IsFalse(ArchImportRules.AppliesTo(null));
        }

        [Test]
        public void QuestBudget_FlagsOverBudget()
        {
            var ok = QuestBudget.Evaluate(40, 100000, 1024);
            Assert.IsTrue(ok.Passes);

            var heavy = QuestBudget.Evaluate(400, 2000000, 4096);
            Assert.IsFalse(heavy.DrawCallsOk);
            Assert.IsFalse(heavy.TrianglesOk);
            Assert.IsFalse(heavy.TextureOk);
            StringAssert.Contains("ALTO", heavy.Summary);
        }

        [Test]
        public void Circuit_CompletesOnceAndUsesLabels()
        {
            var circuit = new CircuitTracker();
            circuit.SetSteps(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("a", "Paso A"),
                new System.Collections.Generic.KeyValuePair<string, string>("b", "Paso B")
            });

            Assert.AreEqual("Paso A", circuit.LabelFor("a"));
            Assert.AreEqual("zzz", circuit.LabelFor("zzz"));
            Assert.IsTrue(circuit.Complete("a"));
            Assert.IsFalse(circuit.Complete("a"));
            Assert.IsFalse(circuit.Complete("desconocido"));
            Assert.IsFalse(circuit.AllDone);
            Assert.IsTrue(circuit.Complete("b"));
            Assert.IsTrue(circuit.AllDone);
        }

        [Test]
        public void PlatformMode_DesktopHidesVrCamera()
        {
            var desktop = PlatformModeSwitch.For(ClientMode.Desktop);
            Assert.IsTrue(desktop.DesktopCamera);
            Assert.IsTrue(desktop.ScreenHud);
            Assert.IsFalse(desktop.XrCamera);
            Assert.IsFalse(desktop.WorldHud);

            var vr = PlatformModeSwitch.For(ClientMode.Vr);
            Assert.IsFalse(vr.DesktopController);
            Assert.IsTrue(vr.XrCamera);
            Assert.IsTrue(vr.XrLocomotion);
            Assert.IsTrue(vr.WorldHud);
        }
    }
}
