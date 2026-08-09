using System;
using UnityEngine;

namespace Ssalddel.Unity.Samples.ResidentialPickup
{
    public sealed class ResidentialPickupSessionTokenProvider : MonoBehaviour
    {
        [NonSerialized]
        private string accessToken = string.Empty;

        public void SetAccessToken(string token)
        {
            accessToken = token?.Trim() ?? string.Empty;
        }

        public string GetAccessToken()
        {
            return accessToken;
        }
    }
}
