using NUnit.Framework;

namespace Metaverso.Tests
{
    public class WorldRulesTests
    {
        const string Page = "https://juliojulioso.github.io/metaverso-mvp/desktop/";

        const string CatalogJson = @"{
  ""version"": 3,
  ""defaultWorld"": ""lobby"",
  ""worlds"": [
    { ""id"": ""Lobby"", ""name"": ""Lobby"", ""address"": ""world/lobby"" },
    { ""id"": ""pabellon-a"", ""name"": ""Pabellon A"", ""access"": ""public"", ""address"": ""world/pabellon-a"",
      ""stands"": [ { ""id"": ""stand-1"", ""name"": ""Stand 1"" } ] },
    { ""id"": ""stand-privado"", ""name"": ""Privado"", ""access"": ""private"", ""address"": ""world/stand-privado"" },
    { ""id"": ""sin-clave"", ""name"": ""Roto"" },
    { ""id"": ""pabellon-a"", ""name"": ""Duplicado"", ""address"": ""world/otro"" }
  ]
}";

        [Test]
        public void UrlState_ReadsAllParams()
        {
            var url = UrlState.Read(Page + "?world=Pabellon-A&spawn=Stand-3&room=Reunion%201&name=Ana%20Ruiz#x");
            Assert.AreEqual("pabellon-a", url.World);
            Assert.AreEqual("stand-3", url.Spawn);
            Assert.AreEqual("reunion1", url.Room);
            Assert.AreEqual("Ana Ruiz", url.Name);
        }

        [Test]
        public void UrlState_MissingParamsAreNull()
        {
            var url = UrlState.Read(Page);
            Assert.IsNull(url.World);
            Assert.IsNull(url.Spawn);
            Assert.IsNull(url.Room);
            Assert.IsNull(url.Name);
            Assert.IsNull(UrlState.Read(null).World);
            Assert.IsNull(UrlState.SanitizeId("!!!"));
        }

        [Test]
        public void UrlState_WithParamReplacesKeepsOthersAndHash()
        {
            Assert.AreEqual(Page + "?name=Ana&world=lobby#top", UrlState.WithParam(Page + "?name=Ana&world=circuito#top", "world", "lobby"));
            Assert.AreEqual(Page + "?world=lobby", UrlState.WithParam(Page, "world", "lobby"));
            Assert.AreEqual(Page + "?name=Ana", UrlState.WithParam(Page + "?spawn=stand-1&name=Ana", "spawn", null));
            Assert.AreEqual(Page, UrlState.WithParam(Page + "?spawn=stand-1", "spawn", null));
        }

        [Test]
        public void UrlState_SearchOfDropsPathAndHash()
        {
            Assert.AreEqual("?world=lobby&room=a", UrlState.SearchOf(Page + "?world=lobby&room=a#h"));
            Assert.AreEqual("", UrlState.SearchOf(Page + "#h"));
        }

        [Test]
        public void UrlState_ShareUrlPointsToSiteRootAndKeepsRoom()
        {
            var share = UrlState.ShareUrl(Page + "?world=circuito&room=sala-1&name=Ana&spawn=x", "pabellon-a");
            Assert.AreEqual("https://juliojulioso.github.io/metaverso-mvp/?world=pabellon-a&room=sala-1", share);
            Assert.AreEqual("https://juliojulioso.github.io/metaverso-mvp/?world=lobby",
                UrlState.ShareUrl("https://juliojulioso.github.io/metaverso-mvp/quest/index.html", "lobby"));
        }

        [Test]
        public void UrlState_DirectoryOfKeepsTrailingSlash()
        {
            Assert.AreEqual(Page, UrlState.DirectoryOf(Page + "index.html?world=a"));
            Assert.AreEqual(Page, UrlState.DirectoryOf(Page + "?world=a"));
            Assert.AreEqual("https://example.com/", UrlState.DirectoryOf("https://example.com"));
        }

        [Test]
        public void UrlState_SessionNameUsesRoomOverWorld()
        {
            Assert.AreEqual("pabellon-a", UrlState.SessionName(UrlState.Read(Page), "pabellon-a"));
            Assert.AreEqual("reunion", UrlState.SessionName(UrlState.Read(Page + "?room=reunion"), "pabellon-a"));
        }

        [Test]
        public void Catalog_ParsesAndDropsInvalidEntries()
        {
            Assert.IsTrue(WorldCatalog.TryParse(CatalogJson, out var catalog, out var warning));
            Assert.AreEqual(3, catalog.Version);
            Assert.AreEqual(3, catalog.Worlds.Count);
            Assert.AreEqual("lobby", catalog.Default.id);
            Assert.AreEqual("Pabellon A", catalog.Find("PABELLON-A").DisplayName);
            Assert.AreEqual(1, catalog.Find("pabellon-a").stands.Length);
            Assert.IsTrue(catalog.Find("stand-privado").IsPrivate);
            Assert.IsNotNull(catalog.Find("lobby").stands);
            StringAssert.Contains("sin-clave", warning);
        }

        [Test]
        public void Catalog_RoutesRequestedDefaultUnknownAndPrivate()
        {
            WorldCatalog.TryParse(CatalogJson, out var catalog, out _);

            var found = catalog.Route("pabellon-a");
            Assert.AreEqual(WorldRouteOutcome.Found, found.Outcome);
            Assert.AreEqual("pabellon-a", found.Entry.id);

            Assert.AreEqual(WorldRouteOutcome.Default, catalog.Route(null).Outcome);
            Assert.AreEqual("lobby", catalog.Route(null).Entry.id);

            var unknown = catalog.Route("nada");
            Assert.AreEqual(WorldRouteOutcome.NotFound, unknown.Outcome);
            Assert.AreEqual("lobby", unknown.Entry.id);

            var locked = catalog.Route("stand-privado");
            Assert.AreEqual(WorldRouteOutcome.NeedsPassword, locked.Outcome);
            Assert.AreEqual("lobby", locked.Entry.id);
            Assert.AreEqual("stand-privado", locked.Requested.id);
        }

        [Test]
        public void Catalog_DefaultFallsBackToFirstPublicWorld()
        {
            const string json = @"{ ""defaultWorld"": ""privado"", ""worlds"": [
                { ""id"": ""privado"", ""access"": ""private"", ""address"": ""world/privado"" },
                { ""id"": ""b"", ""address"": ""world/b"" } ] }";
            Assert.IsTrue(WorldCatalog.TryParse(json, out var catalog, out _));
            Assert.AreEqual("b", catalog.Default.id);
        }

        [Test]
        public void Catalog_RejectsEmptyBrokenOrPrivateOnly()
        {
            Assert.IsFalse(WorldCatalog.TryParse("", out _, out _));
            Assert.IsFalse(WorldCatalog.TryParse("{ no es json", out _, out _));
            Assert.IsFalse(WorldCatalog.TryParse(@"{ ""worlds"": [] }", out _, out _));
            Assert.IsFalse(WorldCatalog.TryParse(@"{ ""worlds"": [ { ""id"": ""p"", ""access"": ""private"", ""address"": ""world/p"" } ] }", out _, out _));
        }

        [Test]
        public void Spawn_PrefersRequestedThenArrivalThenDefault()
        {
            var ids = new[] { "stand-1", "from-lobby", "default" };
            var defaults = new[] { false, false, true };
            Assert.AreEqual(0, SpawnRules.Choose(ids, defaults, "STAND-1", "lobby"));
            Assert.AreEqual(1, SpawnRules.Choose(ids, defaults, "no-existe", "lobby"));
            Assert.AreEqual(2, SpawnRules.Choose(ids, defaults, null, "circuito"));
            Assert.AreEqual(0, SpawnRules.Choose(ids, new[] { false, false, false }, null, null));
            Assert.AreEqual(-1, SpawnRules.Choose(new string[0], new bool[0], "x", "y"));
        }
    }
}
