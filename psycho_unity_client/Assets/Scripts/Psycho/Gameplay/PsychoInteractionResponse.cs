using UnityEngine;

namespace Psycho.Gameplay
{
    public sealed class PsychoInteractionResponse : MonoBehaviour
    {
        [SerializeField] private string defaultMessage;
        [SerializeField] private string talkMessage;
        [SerializeField] private string searchMessage;
        [SerializeField] private string attackMessage;
        [SerializeField] private string examineMessage;

        public void Configure(string fallback, string talk = null, string search = null, string attack = null, string examine = null)
        {
            defaultMessage = fallback;
            talkMessage = talk;
            searchMessage = search;
            attackMessage = attack;
            examineMessage = examine;
        }

        public string BuildMessage(string actionName, string displayName)
        {
            string action = string.IsNullOrWhiteSpace(actionName) ? string.Empty : actionName.ToLowerInvariant();
            if ((action.Contains("talk") || action.Contains("report")) && !string.IsNullOrWhiteSpace(talkMessage))
            {
                return talkMessage;
            }

            if ((action.Contains("take") || action.Contains("search") || action.Contains("read")) && !string.IsNullOrWhiteSpace(searchMessage))
            {
                return searchMessage;
            }

            if (action.Contains("attack") && !string.IsNullOrWhiteSpace(attackMessage))
            {
                return attackMessage;
            }

            if (action.Contains("examine") && !string.IsNullOrWhiteSpace(examineMessage))
            {
                return examineMessage;
            }

            return string.IsNullOrWhiteSpace(defaultMessage) ? actionName + " " + displayName : defaultMessage;
        }
    }
}
