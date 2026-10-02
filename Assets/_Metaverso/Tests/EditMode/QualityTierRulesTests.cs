using NUnit.Framework;

namespace Metaverso.Tests
{
    public class QualityTierRulesTests
    {
        const string Quest3 = "Mozilla/5.0 (X11; Linux x86_64; Quest 3) AppleWebKit/537.36 (KHTML, like Gecko) OculusBrowser/35.2.0.0 SamsungBrowser/4.0 Chrome/128.0.6613.187 VR Safari/537.36";
        const string AndroidPhone = "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Mobile Safari/537.36";
        const string IPhone = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.0 Mobile/15E148 Safari/604.1";
        const string DesktopChrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";

        [Test]
        public void Device_FromUserAgent()
        {
            Assert.AreEqual(DeviceClass.Quest, QualityTierRules.DeviceFromUserAgent(Quest3));
            Assert.AreEqual(DeviceClass.Mobile, QualityTierRules.DeviceFromUserAgent(AndroidPhone));
            Assert.AreEqual(DeviceClass.Mobile, QualityTierRules.DeviceFromUserAgent(IPhone));
            Assert.AreEqual(DeviceClass.Desktop, QualityTierRules.DeviceFromUserAgent(DesktopChrome));
            Assert.AreEqual(DeviceClass.Desktop, QualityTierRules.DeviceFromUserAgent(""));
            Assert.AreEqual(DeviceClass.Desktop, QualityTierRules.DeviceFromUserAgent(null));
        }

        [Test]
        public void Choose_DefaultsByDeviceAndApi()
        {
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Choose(DeviceClass.Quest, WebGraphicsApi.WebGL2, null));
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Choose(DeviceClass.Mobile, WebGraphicsApi.WebGPU, null));
            Assert.AreEqual(QualityTier.Pc, QualityTierRules.Choose(DeviceClass.Desktop, WebGraphicsApi.WebGL2, null));
            Assert.AreEqual(QualityTier.Ultra, QualityTierRules.Choose(DeviceClass.Desktop, WebGraphicsApi.WebGPU, null));
        }

        [Test]
        public void Choose_RequestIsCappedByDevice()
        {
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Choose(DeviceClass.Quest, WebGraphicsApi.WebGL2, QualityTier.Ultra));
            Assert.AreEqual(QualityTier.Pc, QualityTierRules.Choose(DeviceClass.Mobile, WebGraphicsApi.WebGL2, QualityTier.Ultra));
            Assert.AreEqual(QualityTier.Ultra, QualityTierRules.Choose(DeviceClass.Desktop, WebGraphicsApi.WebGL2, QualityTier.Ultra));
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Choose(DeviceClass.Desktop, WebGraphicsApi.WebGPU, QualityTier.Quest));
        }

        [Test]
        public void Parse_AcceptsIdsAndAliases()
        {
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Parse(" Quest "));
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Parse("low"));
            Assert.AreEqual(QualityTier.Pc, QualityTierRules.Parse("PC"));
            Assert.AreEqual(QualityTier.Pc, QualityTierRules.Parse("high"));
            Assert.AreEqual(QualityTier.Ultra, QualityTierRules.Parse("ultra"));
            Assert.IsNull(QualityTierRules.Parse("maxima"));
            Assert.IsNull(QualityTierRules.Parse(""));
            Assert.IsNull(QualityTierRules.Parse(null));
            foreach (var tier in new[] { QualityTier.Quest, QualityTier.Pc, QualityTier.Ultra })
                Assert.AreEqual(tier, QualityTierRules.Parse(QualityTierRules.Id(tier)));
        }

        [Test]
        public void Next_CyclesUpToCeiling()
        {
            Assert.AreEqual(QualityTier.Pc, QualityTierRules.Next(QualityTier.Quest, QualityTier.Ultra));
            Assert.AreEqual(QualityTier.Ultra, QualityTierRules.Next(QualityTier.Pc, QualityTier.Ultra));
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Next(QualityTier.Ultra, QualityTier.Ultra));
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Next(QualityTier.Pc, QualityTier.Pc));
            Assert.AreEqual(QualityTier.Quest, QualityTierRules.Next(QualityTier.Quest, QualityTier.Quest));
        }

        [Test]
        public void Profiles_QuestIsLeanAndTiersGrow()
        {
            var quest = QualityTierRules.Profile(QualityTier.Quest);
            var pc = QualityTierRules.Profile(QualityTier.Pc);
            var ultra = QualityTierRules.Profile(QualityTier.Ultra);

            Assert.IsFalse(quest.Hdr);
            Assert.IsFalse(quest.PostProcessing);
            Assert.IsFalse(quest.Ssao);
            Assert.IsFalse(quest.AdditionalLightShadows);
            Assert.AreEqual(1f, quest.MaxPixelRatio);
            Assert.AreEqual("Web_Quest_Renderer", quest.Renderer);
            Assert.AreEqual("Web_PC_Renderer", pc.Renderer);
            Assert.AreEqual(pc.Renderer, ultra.Renderer);
            Assert.AreEqual(pc.Ssao, ultra.Ssao, "Web_PC_Renderer es compartido: SSAO tiene que coincidir.");

            Assert.LessOrEqual(quest.MainShadowResolution, pc.MainShadowResolution);
            Assert.LessOrEqual(pc.MainShadowResolution, ultra.MainShadowResolution);
            Assert.LessOrEqual(quest.ShadowDistance, pc.ShadowDistance);
            Assert.LessOrEqual(pc.ShadowDistance, ultra.ShadowDistance);
            Assert.LessOrEqual(pc.MaxPixelRatio, ultra.MaxPixelRatio);

            CollectionAssert.AllItemsAreUnique(new[] { quest.QualityLevel, pc.QualityLevel, ultra.QualityLevel });
            CollectionAssert.AllItemsAreUnique(new[] { quest.PipelineAsset, pc.PipelineAsset, ultra.PipelineAsset });
            CollectionAssert.AreEqual(QualityTierRules.QualityLevels, new[] { quest.QualityLevel, pc.QualityLevel, ultra.QualityLevel });
        }
    }
}
