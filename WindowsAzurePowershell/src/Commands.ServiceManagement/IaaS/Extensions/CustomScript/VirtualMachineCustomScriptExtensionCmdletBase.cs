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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Linq;
    using Management.Storage;
    using Model.PersistentVMModel;
    using Newtonsoft.Json;
    using Utilities.Common;

    public class VirtualMachineCustomScriptExtensionCmdletBase : VirtualMachineExtensionCmdletBase
    {
        protected const string VirtualMachineCustomScriptExtensionNoun = "AzureVMCustomScriptExtension";
        protected const string ExtensionDefaultPublisher = "Microsoft.Compute";
        protected const string ExtensionDefaultName = "CustomScriptExtension";
        protected const string LegacyReferenceName = "MyCustomScriptExtension";

        public virtual string ContainerName { get; set; }
        public virtual string[] FileName { get; set; }
        public virtual string[] FileUri{ get; set; }
        public virtual string StorageAccountName { get; set; }
        public virtual string StorageAccountKey { get; set; }
        public virtual string StorageEndpointSuffix { get; set; }
        public virtual string Run { get; set; }
        public virtual string Argument { get; set; }

        public VirtualMachineCustomScriptExtensionCmdletBase()
        {
            base.publisherName = ExtensionDefaultPublisher;
            base.extensionName = ExtensionDefaultName;
        }

        protected string GetPublicConfiguration()
        {
            const string poshCmdFormatStr = "powershell {0} -file {1} {2}";
            const string defaultPolicyStr = "Unrestricted";
            const string policyFormatStr = "-ExecutionPolicy {0}";

            string policyStr = string.Format(policyFormatStr, defaultPolicyStr);

            return JsonUtilities.TryFormatJson(JsonConvert.SerializeObject(
               new PublicSettings
               {
                   fileUris = this.FileUri,
                   commandToExecute = string.Format(poshCmdFormatStr, policyStr, this.Run, this.Argument)
               }));
        }

        protected string GetPrivateConfiguration()
        {
            return string.IsNullOrEmpty(this.StorageAccountName)|| string.IsNullOrEmpty(this.StorageAccountKey)
                 ? string.Empty
                 : JsonUtilities.TryFormatJson(JsonConvert.SerializeObject(
                   new PrivateSettings
                   {
                       storageAccountName = this.StorageAccountName,
                       storageAccountKey = this.StorageAccountKey ?? string.Empty
                   }));
        }

        protected virtual void GetExtensionValues(ResourceExtensionReference extensionRef)
        {
            if (extensionRef != null && extensionRef.ResourceExtensionParameterValues != null)
            {
                Disable = string.Equals(extensionRef.State, ReferenceDisableStr);
                GetExtensionValues(extensionRef.ResourceExtensionParameterValues);
            }
            else
            {
                Disable = extensionRef == null ? true : string.Equals(extensionRef.State, ReferenceDisableStr);
            }
        }

        protected virtual void GetExtensionValues(ResourceExtensionParameterValueList paramVals)
        {
            if (paramVals != null && paramVals.Any())
            {
                var publicParamVal = paramVals.FirstOrDefault(
                    r => !string.IsNullOrEmpty(r.Value) && string.Equals(r.Type, PublicTypeStr));
                if (publicParamVal != null && !string.IsNullOrEmpty(publicParamVal.Value))
                {
                    this.PublicConfiguration = publicParamVal.Value;
                }

                var privateParamVal = paramVals.FirstOrDefault(
                    r => !string.IsNullOrEmpty(r.Value) && string.Equals(r.Type, PrivateTypeStr));
                if (privateParamVal != null && !string.IsNullOrEmpty(privateParamVal.Value))
                {
                    this.PrivateConfiguration = privateParamVal.Value;
                }
            }
        }
    }
}
