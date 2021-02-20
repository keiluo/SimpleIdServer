﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace SimpleIdServer.OpenBankingApi
{
    public static class Constants
    {
        public static class AuthorizationPolicies
        {
            public const string Accounts = "accounts";
        }

        public static class RouteNames
        {
            public const string AccountAccessContents = ApiVersion + "/account-access-consents";
        }

        public const string ApiVersion = "v3.1";
    }
}
