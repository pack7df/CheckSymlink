﻿using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace Spt.Shared.AspNetCore
{
    public class CloudRoleNameInitializer : ITelemetryInitializer
    {
        private readonly string _roleName;

        public CloudRoleNameInitializer(string roleName)
        {
            _roleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = _roleName;
        }
    }
}
