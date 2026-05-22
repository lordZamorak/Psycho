using UnityEngine;

namespace Psycho.Mirror
{
    public sealed class PsychoMirrorLoader : MonoBehaviour
    {
        [SerializeField] private bool loadOnAwake = true;
        [SerializeField] private bool logSummary = true;
        [SerializeField] private string streamingAssetsOverride = "";

        public PsychoMirrorDatabase Database { get; private set; }

        private void Awake()
        {
            if (loadOnAwake)
            {
                Load();
            }
        }

        public PsychoMirrorDatabase Load()
        {
            Database = PsychoMirrorDatabase.LoadFromStreamingAssets(streamingAssetsOverride);
            if (logSummary)
            {
                Debug.Log(Database.GetSummary());
            }

            return Database;
        }
    }
}
