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

using Microsoft.Azure.Commands.ResourceManager.Models;
using Microsoft.Azure.Commands.ResourceManager.Properties;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ResourceManager
{
    public abstract class ResourceWithParameterBaseCmdlet : ResourceManagerBaseCmdlet
    {
        protected const string BaseParameterSetName = "Default";
        protected const string GalleryTemplateParameterObjectParameterSetName = "Deployment via Gallery and template parameters object";
        protected const string GalleryTemplateParameterFileParameterSetName = "Deployment via Gallery and template parameters file";
        protected const string TemplateFileParameterObjectParameterSetName = "Deployment via template file and template parameters object";
        protected const string TemplateFileParameterFileParameterSetName = "Deployment via template file and template parameters file";
        protected const string TemplateUriParameterObjectParameterSetName = "Deployment via template uri and template parameters object";
        protected const string TemplateUriParameterFileParameterSetName = "Deployment via template uri and template parameters file";
        protected const string ParameterlessTemplateFileParameterSetName = "Deployment via template file without parameters";
        protected const string ParameterlessGalleryTemplateParameterSetName = "Deployment via Gallery without parameters";
        protected const string ParameterlessTemplateUriParameterSetName = "Deployment via template uri without parameters";
        
        protected RuntimeDefinedParameterDictionary dynamicParameters;

        private string galleryTemplateName;

        private string templateFile;

        private string templateUri;

        protected ResourceWithParameterBaseCmdlet()
        {
            dynamicParameters = new RuntimeDefinedParameterDictionary();
            galleryTemplateName = null;
        }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents the parameters.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents the parameters.")]
        [Parameter(ParameterSetName = TemplateUriParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents the parameters.")]
        public Hashtable TemplateParameterObject { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A file that has the template parameters.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A file that has the template parameters.")]
        [Parameter(ParameterSetName = TemplateUriParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A file that has the template parameters.")]
        [ValidateNotNullOrEmpty]
        public string TemplateParameterFile { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [Parameter(ParameterSetName = ParameterlessGalleryTemplateParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [ValidateNotNullOrEmpty]
        public string GalleryTemplateIdentity { get; set; }

        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Local path to the template file.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Local path to the template file.")]
        [Parameter(ParameterSetName = ParameterlessTemplateFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Local path to the template file.")]
        [ValidateNotNullOrEmpty]
        public string TemplateFile { get; set; }

        [Parameter(ParameterSetName = TemplateUriParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Uri to the template file.")]
        [Parameter(ParameterSetName = TemplateUriParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Uri to the template file.")]
        [Parameter(ParameterSetName = ParameterlessTemplateUriParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Uri to the template file.")]
        [ValidateNotNullOrEmpty]
        public string TemplateUri { get; set; }

        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [Parameter(ParameterSetName = ParameterlessTemplateFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content version of the template.")]
        [ValidateNotNullOrEmpty]
        public string TemplateVersion { get; set; }

        public object GetDynamicParameters()
        {
            if (!string.IsNullOrEmpty(GalleryTemplateIdentity) &&
                !GalleryTemplateIdentity.Equals(galleryTemplateName, StringComparison.OrdinalIgnoreCase))
            {
                galleryTemplateName = GalleryTemplateIdentity;
                try
                {
                    dynamicParameters = GalleryTemplatesClient.GetTemplateParametersFromGallery(
                        GalleryTemplateIdentity,
                        TemplateParameterObject,
                        this.TryResolvePath(TemplateParameterFile),
                        MyInvocation.MyCommand.Parameters.Keys.ToArray());
                }
                catch (CloudException)
                {
                    throw new ArgumentException(string.Format(Resources.UnableToFindGallery, GalleryTemplateIdentity));
                }
            }
            else if (!string.IsNullOrEmpty(TemplateFile) &&
                !TemplateFile.Equals(templateFile, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    templateFile = TemplateFile;
                    dynamicParameters = GalleryTemplatesClient.GetTemplateParametersFromFile(
                        this.TryResolvePath(TemplateFile),
                        TemplateParameterObject,
                        this.TryResolvePath(TemplateParameterFile),
                        MyInvocation.MyCommand.Parameters.Keys.ToArray());
                } 
                catch
                {
                    throw new ArgumentException(string.Format(Resources.FailedToParseProperty, "TemplateFile", TemplateFile));
                }
            }
            else if (!string.IsNullOrEmpty(TemplateUri) &&
                !TemplateUri.Equals(templateUri, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    templateUri = TemplateUri;
                    dynamicParameters = GalleryTemplatesClient.GetTemplateParametersFromFile(
                        TemplateUri,
                        TemplateParameterObject,
                        this.TryResolvePath(TemplateParameterFile),
                        MyInvocation.MyCommand.Parameters.Keys.ToArray());
                }
                catch
                {
                    throw new ArgumentException(string.Format(Resources.FailedToParseProperty, "TemplateUri", TemplateUri));
                }
            }

            return dynamicParameters;
        }

        protected Hashtable GetTemplateParameterObject(Hashtable templateParameterObject)
        {
            templateParameterObject = templateParameterObject ?? new Hashtable();

            // Load parameters from the file
            string templateParameterFilePath = this.TryResolvePath(TemplateParameterFile);
            if (templateParameterFilePath != null && File.Exists(templateParameterFilePath))
            {
                var parametersFromFile = JsonConvert.DeserializeObject<Dictionary<string, TemplateFileParameter>>(File.ReadAllText(templateParameterFilePath));
                parametersFromFile.ForEach(dp => templateParameterObject[dp.Key] = dp.Value.Value);
            }

            // Load dynamic parameters
            IEnumerable<RuntimeDefinedParameter> parameters = PowerShellUtilities.GetUsedDynamicParameters(dynamicParameters, MyInvocation);
            if (parameters.Any())
            {
                parameters.ForEach(dp => templateParameterObject[((ParameterAttribute)dp.Attributes[0]).HelpMessage] = dp.Value);
            }

            return templateParameterObject;
        }
    }
}
