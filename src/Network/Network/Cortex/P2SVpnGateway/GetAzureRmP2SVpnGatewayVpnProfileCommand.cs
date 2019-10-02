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

namespace Microsoft.Azure.Commands.Network
{
    using AutoMapper;
    using Microsoft.Azure.Commands.Network.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using Newtonsoft.Json;
    using MNM = Microsoft.Azure.Management.Network.Models;
    using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

    [Cmdlet("Get", ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "P2sVpnGatewayVpnProfile", SupportsShouldProcess = true), OutputType(typeof(PSVpnProfileResponse))]
    public class GetAzureRmP2SVpnGatewayVpnProfileCommand : P2SVpnGatewayBaseCmdlet
    {
        [Alias("ResourceName")]
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource name.")]
        [ResourceNameCompleter("Microsoft.Network/p2sVpnGateways", "ResourceGroupName")]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Authentication Method")]
        [ValidateSet(
            MNM.AuthenticationMethod.EAPTLS,
            MNM.AuthenticationMethod.EAPMSCHAPv2,
            IgnoreCase = true)]
        public string AuthenticationMethod { get; set; }

        public override void Execute()
        {
            base.Execute();
            string shouldProcessMessage = string.Format("Execute Get-AzureRmP2sVpnGatewayVpnProfile for ResourceGroupName {0} P2SVpnGateway {1}", ResourceGroupName, Name);
            if (ShouldProcess(shouldProcessMessage, VerbsCommon.Get))
            {
                PSP2SVpnProfileParameters p2sVpnProfileParams = new PSP2SVpnProfileParameters();

                p2sVpnProfileParams.AuthenticationMethod = string.IsNullOrWhiteSpace(this.AuthenticationMethod)
                    ? MNM.AuthenticationMethod.EAPTLS.ToString()
                    : this.AuthenticationMethod;

                var p2sVpnProfileParametersModel = NetworkResourceManagerProfile.Mapper.Map<MNM.P2SVpnProfileParameters>(p2sVpnProfileParams);

                // There may be a required Json serialize for the package URL to conform to REST-API
                // The try-catch below handles the case till the change is made and deployed to PROD
                string serializedPackageUrl = this.NetworkClient.GenerateP2SVpnGatewayVpnProfile(this.ResourceGroupName, this.Name, p2sVpnProfileParametersModel);
                MNM.VpnProfileResponse p2sVpnGatewayVpnProfile = new MNM.VpnProfileResponse();
                try
                {
                    p2sVpnGatewayVpnProfile = JsonConvert.DeserializeObject<MNM.VpnProfileResponse>(serializedPackageUrl);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                PSVpnProfileResponse vpnProfileResponse = new PSVpnProfileResponse() { ProfileUrl = p2sVpnGatewayVpnProfile?.ProfileUrl };
                WriteObject(vpnProfileResponse);
            }
        }
    }
}
