using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class RegionPlacementPreviewEditModeTests
    {
        [Test]
        public void Preview_RevalidatesAtCommit_SnapsAndCallsConsumerOnce()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session,new Rect(-10,-10,20,20)))
            {
                int calls = 0;
                using (var preview = new RegionPlacementPreview(region, Vector2.one, (p,y) => { calls++; Assert.That(p,Is.EqualTo(new Vector2(2.5f,0))); Assert.That(y,Is.EqualTo(15)); }))
                {
                    Assert.That(preview.Confirm(), Is.False);
                    preview.Move(new Vector2(2.26f,0)); preview.Rotate();
                    Assert.That(preview.IsValid, Is.True);
                    var blocker = region.Register(PersistentObjectKind.Building,"障碍",new Vector2(2.5f,0),Vector2.one);
                    Assert.That(preview.Confirm(), Is.False);
                    region.Remove(blocker.Id);
                    Assert.That(preview.Confirm(), Is.True);
                    Assert.That(preview.Confirm(), Is.False);
                    Assert.That(calls,Is.EqualTo(1));
                }
                using (var cancelled = new RegionPlacementPreview(region,Vector2.one,(p,y) => calls++))
                { cancelled.Move(Vector2.zero); cancelled.Dispose(); Assert.That(cancelled.Confirm(),Is.False); }
                Assert.That(calls, Is.EqualTo(1));
            }
        }
    }
}
