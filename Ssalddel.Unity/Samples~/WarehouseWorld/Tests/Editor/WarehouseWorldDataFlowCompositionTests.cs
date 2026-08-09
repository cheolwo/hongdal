using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Ssalddel.Unity.Warehouse;
using Ssalddel.Unity.Npcs;
using VContainer;

namespace Ssalddel.Unity.Samples.WarehouseWorld.EditorTests
{
    public sealed class WarehouseWorldDataFlowCompositionTests
    {
        [Test]
        public async Task VContainer는_DataRepository_Interpreter_QueryUseCase를조립한다()
        {
            var client = new FixtureClient();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(client).As<IWarehouseWorldApiClient>();
            builder.Register<NpcMovementMapper>(Lifetime.Scoped);
            builder.Register<CargoWarehouseHandoffMapper>(Lifetime.Scoped);
            builder.Register<WarehouseDataMapper>(Lifetime.Scoped);
            builder.Register<WarehouseApiDataRepository>(Lifetime.Scoped).As<IWarehouseDataRepository>();
            builder.Register<WarehouseInboundHandoffInterpreter>(Lifetime.Scoped);
            builder.Register<WarehouseWorldInterpreter>(Lifetime.Scoped);
            builder.Register<WarehouseWorldMapper>(Lifetime.Scoped);
            builder.Register<WarehouseWorldApiRepository>(Lifetime.Scoped).As<IWarehouseWorldRepository>();
            builder.Register<WarehouseWorldQueryUseCase>(Lifetime.Scoped);

            using var resolver = builder.Build();
            var useCase = resolver.Resolve<WarehouseWorldQueryUseCase>();
            var result = await useCase.실행Async(7);

            Assert.That(client.CallCount, Is.EqualTo(1));
            Assert.That(result.Lineage, Is.Not.Null);
            Assert.That(result.Objects, Has.Length.EqualTo(1));
            Assert.That(result.Objects[0].StableId, Is.EqualTo("warehouse-inventory:31"));
        }

        [Test]
        public async Task VContainer는_WarehouseW2_Handoff를_차량화물Dock관계로조립한다()
        {
            var builder = new ContainerBuilder();
            builder.Register<SimulatedWarehouseWorldApiClient>(Lifetime.Scoped).As<IWarehouseWorldApiClient>();
            builder.Register<NpcMovementMapper>(Lifetime.Scoped);
            builder.Register<CargoWarehouseHandoffMapper>(Lifetime.Scoped);
            builder.Register<WarehouseDataMapper>(Lifetime.Scoped);
            builder.Register<WarehouseApiDataRepository>(Lifetime.Scoped).As<IWarehouseDataRepository>();
            builder.Register<WarehouseInboundHandoffInterpreter>(Lifetime.Scoped);
            builder.Register<WarehouseWorldInterpreter>(Lifetime.Scoped);
            builder.Register<WarehouseWorldQueryUseCase>(Lifetime.Scoped);

            using var resolver = builder.Build();
            var result = await resolver.Resolve<WarehouseWorldQueryUseCase>().실행Async(7);
            var cargo = Array.Find(result.Objects, item => item.Kind == "Cargo");
            var vehicle = Array.Find(result.Objects, item => item.Kind == "Vehicle");

            Assert.That(cargo, Is.Not.Null);
            Assert.That(vehicle, Is.Not.Null);
            Assert.That(cargo!.LocationCode, Is.EqualTo(WarehouseLocationSocketKeys.InboundDock));
            Assert.That(vehicle!.CanonicalRelationStableId, Is.EqualTo(cargo.CanonicalRelationStableId));
        }

        private sealed class FixtureClient : IWarehouseWorldApiClient
        {
            public int CallCount { get; private set; }

            public Task<WarehouseWorldSnapshotApiModel> GetAsync(
                long warehouseId,
                CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult(new WarehouseWorldSnapshotApiModel
                {
                    StableId = "warehouse-zone:7",
                    Revision = "revision-composition-1",
                    GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                    TotalAvailableQuantity = 12,
                    InventoryItems = new[]
                    {
                        new WarehouseWorldInventoryItemApiModel
                        {
                            StableId = "warehouse-inventory:31",
                            WarehouseStableId = "warehouse:7",
                            WarehouseName = "도심 창고",
                            ProductName = "감자",
                            AvailableQuantity = 12,
                            StorageLocation = "A-01",
                            Status = "검수완료",
                            UpdatedAtUtc = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                        },
                    },
                });
            }
        }
    }
}
