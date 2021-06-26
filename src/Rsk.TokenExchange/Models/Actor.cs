namespace Rsk.TokenExchange
{
    /// <summary>
    /// Object representing the actor (act) claim.
    /// Useful for creating an audit trail during token exchange.
    /// </summary>
    public class Actor
    {
        /// <summary>
        /// The original issuer (iss) of exchanged token.
        /// Useful for cross-tenant/identity provider token exchange.
        /// </summary>
        public string Issuer { get; set; }
        
        /// <summary>
        /// The original client ID (client_id) of the exchanged token.
        /// Useful for tracking who originally requested the exchanged token. 
        /// </summary>
        public string ClientId { get; set; }
        
        /// <summary>
        /// The inner actor claim.
        /// Useful for multiple token exchange requests.
        /// </summary>
        public Actor InnerActor { get; set; }
        
    }
}