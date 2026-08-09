using System;
using System.Collections;
using NUnit.Framework;
using Ssalddel.Unity.Warehouse;
using UnityEngine;
using UnityEngine.TestTools;

namespace Ssalddel.Unity.Samples.WarehouseWorld.EditorTests
{
    public sealed class WarehouseWorldOperationalRefreshTests
    {
        [UnityTest]
        public IEnumerator OperationalApi는_실제UnityWebRequest갱신후_단절시마지막성공Snapshot을유지한다()
        {
            var baseUrl = Setting("SSALDDEL_WAREHOUSE_W1_BASE_URL", "-warehouseW1BaseUrl");
            var accessToken = Setting("SSALDDEL_WAREHOUSE_W1_ACCESS_TOKEN", "-warehouseW1AccessToken");
            var warehouseIdText = Setting("SSALDDEL_WAREHOUSE_W1_WAREHOUSE_ID", "-warehouseW1WarehouseId");
            long warehouseId;
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(accessToken) || !long.TryParse(warehouseIdText, out warehouseId))
            {
                Assert.Ignore("Warehouse W1 operational probe environment is not configured.");
                yield break;
            }

            var tokenObject = new GameObject("WarehouseOperationalProbeTokenProvider");
            var tokenProvider = tokenObject.AddComponent<WarehouseRuntimeSessionTokenProvider>();
            WarehouseRuntimeSessionTokenProvider.SetAccessToken(accessToken);
            var options = new WarehouseWorldApiOptions { BaseUrl = baseUrl, TimeoutSeconds = 5 };
            var client = new OperationalWarehouseWorldApiClient(options, tokenProvider);
            var repository = new WarehouseWorldApiRepository(client, new WarehouseWorldMapper());
            var coordinator = new WarehouseWorldLoadCoordinator(new WarehouseWorldQueryUseCase(repository), new WarehouseWorldReconciler());

            try
            {
                var firstTask = coordinator.LoadAsync(warehouseId);
                while (!firstTask.IsCompleted) yield return null;
                var first = firstTask.GetAwaiter().GetResult();
                Assert.That(first.StateCode, Is.EqualTo(WarehouseWorldLoadStateCodes.Success));
                Assert.That(first.Snapshot, Is.Not.Null);
                Assert.That(first.Snapshot!.Objects, Is.Not.Empty);

                var refreshTask = coordinator.LoadAsync(warehouseId);
                while (!refreshTask.IsCompleted) yield return null;
                var refreshed = refreshTask.GetAwaiter().GetResult();
                Assert.That(refreshed.StateCode, Is.EqualTo(WarehouseWorldLoadStateCodes.Success));
                Assert.That(refreshed.Snapshot!.Revision, Is.EqualTo(first.Snapshot.Revision));
                Assert.That(refreshed.Changes!.Added, Is.Empty);
                Assert.That(refreshed.Changes.Updated, Is.Empty);
                Assert.That(refreshed.Changes.Removed, Is.Empty);

                options.BaseUrl = "http://127.0.0.1:1/";
                options.TimeoutSeconds = 1;
                var failureTask = coordinator.LoadAsync(warehouseId);
                while (!failureTask.IsCompleted) yield return null;
                var refreshFailure = failureTask.GetAwaiter().GetResult();
                Assert.That(refreshFailure.StateCode, Is.EqualTo(WarehouseWorldLoadStateCodes.RefreshError));
                Assert.That(refreshFailure.Snapshot, Is.SameAs(refreshed.Snapshot));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tokenObject);
            }
        }

        private static string Setting(string environmentName, string argumentName)
        {
            var environmentValue = Environment.GetEnvironmentVariable(environmentName);
            if (!string.IsNullOrWhiteSpace(environmentValue)) return environmentValue;
            var arguments = Environment.GetCommandLineArgs();
            for (var index = 0; index < arguments.Length - 1; index++)
                if (string.Equals(arguments[index], argumentName, StringComparison.Ordinal)) return arguments[index + 1];
            return null;
        }
    }
}
