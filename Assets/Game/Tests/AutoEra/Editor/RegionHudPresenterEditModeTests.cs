using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class RegionHudPresenterEditModeTests
    {
        [Test]
        public void Projection_TracksOnlySelectedPublicState_AndUnsubscribes()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-10, -10, 20, 20)))
            using (var presenter = new RegionHudPresenter(region))
            {
                var first = region.Register(PersistentObjectKind.ResourcePoint, "水源", Vector2.zero, Vector2.one);
                var next = region.Register(PersistentObjectKind.Machine, "载体", new Vector2(3, 0), Vector2.one);
                Assert.That(presenter.ObjectCount, Is.EqualTo(2));
                int changes = 0;
                presenter.Changed += () => changes++;
                Assert.That(presenter.HasSelection, Is.False);
                region.Select(first.Id, false);
                Assert.That(presenter.ObjectSummary, Does.Contain("水源"));
                Assert.That(presenter.ObjectSummary, Does.Not.Contain("可用资源"));
                first.SetPublicState("可取水", infinite: true);
                Assert.That(presenter.ObjectSummary, Does.Contain("资源：无限"));
                region.Select(next.Id, false);
                string selectedSummary = presenter.ObjectSummary;
                first.SetPublicState("变化");
                Assert.That(presenter.ObjectSummary, Is.EqualTo(selectedSummary));
                region.Remove(next.Id);
                Assert.That(presenter.ObjectCount, Is.EqualTo(1));
                Assert.That(changes, Is.GreaterThan(0));
                Assert.That(presenter.ObjectSummary, Is.Empty);
                Assert.That(presenter.HasSelection, Is.False);
                presenter.Dispose();
                int before = changes;
                region.Select(first.Id, false);
                region.Remove(first.Id);
                Assert.That(changes, Is.EqualTo(before));
                Assert.That(presenter.ObjectCount, Is.Zero);
                Assert.That(presenter.ObjectSummary, Is.Empty);
            }
        }
    }
}
