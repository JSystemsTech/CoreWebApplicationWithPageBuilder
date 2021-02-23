using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuthNAuthZ.TokenProvider
{
    public class TokenClaim
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public IEnumerable<string> Values { get; set; }
        public TokenClaim() { }
        internal TokenClaim(string name, IEnumerable<string> values)
        {
            Name = name;
            Values = values.Count() == 1 ? values.First().Split(',') : values;
            Value = string.Join(",", Values);
        }
        internal IEnumerable<string> GetValues() => Values != null ? Values : !string.IsNullOrWhiteSpace(Value) ? Value.Split(',') : new string[0];
        internal string GetValue() => string.Join(",", GetValues());
    }
    public interface ITokenHandler
    {
        string Create(IEnumerable<TokenClaim> claims);
        string Renew(string tokenStr, IEnumerable<TokenClaim> claims);
        string Renew(string tokenStr, TokenClaim claim);
        IEnumerable<TokenClaim> GetClaims(string tokenStr);
        DateTime? GetExpirationDate(string tokenStr);
        bool IsValid(string tokenStr);
    }
    public abstract class TokenHandler : ITokenHandler
    {
        public abstract string Create(IEnumerable<TokenClaim> claims);

        public abstract IEnumerable<TokenClaim> GetClaims(string tokenStr);

        public abstract DateTime? GetExpirationDate(string tokenStr);

        public abstract bool IsValid(string tokenStr);

        public abstract string Renew(string tokenStr, IEnumerable<TokenClaim> claims);

        public abstract string Renew(string tokenStr, TokenClaim claim);
    }

    public interface ITokenProviderServiceSettings
    {
        Uri AudienceUri { get; }
        string ConfirmationMethod { get; }
        string Issuer { get; }
        string Namespace { get; }
        string SubjectName { get; }
        int ValidFor { get; }
    }
    public sealed class TokenProviderServiceSettings : ITokenProviderServiceSettings
    {
        public string AudienceUrl { get; set; }
        public Uri AudienceUri => new Uri(AudienceUrl);
        public string ConfirmationMethod { get; set; }
        public string Issuer { get; set; }
        public string Namespace { get; set; }
        public string SubjectName { get; set; }
        public int ValidFor { get; set; }
        public TokenProviderServiceSettings() : base() { }
    }
}
