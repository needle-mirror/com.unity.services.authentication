using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Contains Player Information
    /// </summary>
    public sealed class PlayerInfo
    {
        /// <summary>
        /// Player Id
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Player Creation DateTime in UTC
        /// </summary>
        public DateTime? CreatedAt { get; }

        /// <summary>
        /// Player Identities
        /// </summary>
        public List<Identity> Identities { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        internal PlayerInfo(string playerId)
        {
            Id = playerId;
            Identities = new List<Identity>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal PlayerInfo(PlayerInfoResponse response) : this(response.Id, response.CreatedAt, response.ExternalIds)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal PlayerInfo(User user) : this(user.Id, user.CreatedAt, user.ExternalIds)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal PlayerInfo(string playerId, string createdAt, List<ExternalIdentity> externalIdentities)
        {
            Id = playerId;
            Identities = new List<Identity>();

            if (double.TryParse(createdAt, out var createAtSeconds))
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                CreatedAt = epoch.AddSeconds(createAtSeconds);
            }

            if (externalIdentities != null)
            {
                foreach (var externalId in externalIdentities)
                {
                    Identities.Add(new Identity(externalId));
                }
            }
        }

        /// <summary>
        /// Returns the player's facebook id if one has been linked.
        /// </summary>
        /// <returns>The player's facebook id</returns>
        public string GetFacebookId()
        {
            return GetIdentityId(IdProviderKeys.Facebook);
        }

        /// <summary>
        /// Returns the player's steam id if one has been linked.
        /// </summary>
        /// <returns></returns>
        public string GetSteamId()
        {
            return GetIdentityId(IdProviderKeys.Steam);
        }

        /// <summary>
        /// Returns the player's Google Play Games id if one has been linked.
        /// </summary>
        /// <returns>The player's Google Play Games id</returns>
        public string GetGoogleId()
        {
            return GetIdentityId(IdProviderKeys.Google);
        }

        /// <summary>
        /// Returns the player's Sign in with Apple id if one has been linked.
        /// </summary>
        /// <returns>The player's Sign in with Apple id</returns>
        public string GetAppleId()
        {
            return GetIdentityId(IdProviderKeys.Apple);
        }

        /// <summary>
        /// Returns the player's identity user id for a given identity type id
        /// </summary>
        internal string GetIdentityId(string typeId)
        {
            return Identities?.FirstOrDefault(x => x.TypeId == typeId)?.UserId;
        }

        /// <summary>
        /// Add External Id to the Player Info
        /// </summary>
        internal void AddExternalIdentity(ExternalIdentity externalId)
        {
            if (externalId != null)
            {
                Identities.Add(new Identity(externalId));
            }
        }

        /// <summary>
        /// Removes External Id
        /// </summary>
        internal void RemoveIdentity(string typeId)
        {
            Identities?.RemoveAll(x => x.TypeId == typeId);
        }
    }
}
