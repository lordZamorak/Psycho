using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Psycho.Networking
{
    [RequireComponent(typeof(PsychoProtocolClient))]
    public sealed class PsychoNetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private string settingsPath = "PsychoClient/server.json";
        [SerializeField] private bool autoProbeOnStart;

        private PsychoProtocolClient client;
        private CancellationTokenSource cancellation;

        private async void Awake()
        {
            client = GetComponent<PsychoProtocolClient>();
            client.StatusChanged += status => Debug.Log($"Psycho network: {status}");

            PsychoServerSettings settings = LoadSettings();
            if (settings != null)
            {
                client.Configure(settings.host, settings.port, settings.clientVersion);
                autoProbeOnStart = settings.autoProbeOnStart;
            }

            if (!autoProbeOnStart)
            {
                return;
            }

            cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            try
            {
                await client.ProbeAsync(cancellation.Token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Psycho network probe failed: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            cancellation?.Cancel();
            cancellation?.Dispose();
        }

        private PsychoServerSettings LoadSettings()
        {
            string path = Path.Combine(Application.streamingAssetsPath, settingsPath);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Psycho network settings missing: {path}");
                return null;
            }

            return JsonUtility.FromJson<PsychoServerSettings>(File.ReadAllText(path));
        }

        [Serializable]
        private sealed class PsychoServerSettings
        {
            public string host = "127.0.0.1";
            public int port = 13377;
            public int clientVersion = 55;
            public bool autoProbeOnStart;
        }
    }
}
