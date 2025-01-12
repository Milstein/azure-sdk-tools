﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Model
{
    using System.Collections.Generic;
    using PersistentVMModel;

    public class PersistentVMRoleContext : ServiceOperationContext, IPersistentVM
    {
        public string DeploymentName { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public PersistentVM VM { get; set; }
        public string InstanceStatus { get; set; }
        public string IpAddress { get; set; }
        public string InstanceStateDetails { get; set; }
        public string PowerState { get; set; }
        public string InstanceErrorCode { get; set; }
        public string InstanceFaultDomain { get; set; }
        public string InstanceName { get; set; }
        public string InstanceUpgradeDomain { get; set; }
        public string InstanceSize { get; set; }
        public string AvailabilitySetName { get; set; }
        public string DNSName { get; set; }
        public string Status { get; set; }
        public GuestAgentStatus GuestAgentStatus { get; set; }
        public List<ResourceExtensionStatus> ResourceExtensionStatusList { get; set; }

        public PersistentVM GetInstance()
        {
            return this.VM;
        }
    }
}