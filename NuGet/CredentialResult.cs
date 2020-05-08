namespace NuGet
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;

    internal class CredentialResult
    {
        public static CredentialResult Create(CredentialState state, ICredentials credentials)
        {
            CredentialResult result1 = new CredentialResult();
            result1.State = state;
            result1.Credentials = credentials;
            return result1;
        }

        public CredentialState State { get; set; }

        public ICredentials Credentials { get; set; }
    }
}

