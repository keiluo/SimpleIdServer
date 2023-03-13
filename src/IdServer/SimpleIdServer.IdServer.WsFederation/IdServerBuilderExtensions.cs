﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using SimpleIdServer.IdServer.WsFederation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdServerBuilderExtensions
    {
        public static IdServerBuilder AddWsFederation(this IdServerBuilder idServerBuilder, Action<IdServerWsFederationOptions> callback = null)
        {
            if (callback == null) idServerBuilder.Services.Configure<IdServerWsFederationOptions>((o) => { });
            else idServerBuilder.Services.Configure(callback);
            return idServerBuilder;
        }
    }
}
