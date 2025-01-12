﻿//
// Copyright (c) Microsoft.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

namespace Microsoft.WindowsAzure.Commands.Common.Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.WindowsAzure.Common;
    using Microsoft.WindowsAzure.Common.Internals;
    using System.Diagnostics;

    public class TestingTracingInterceptor : ICloudTracingInterceptor
    {
        public TestingTracingInterceptor()
        {
#if DEBUG
            Debug.Listeners.Add(new DefaultTraceListener());
#endif
        }

        private void Write(string message, params object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                Console.WriteLine(message);
#if DEBUG
                Debug.WriteLine(message);

#endif
            }
            else
            {
                Console.WriteLine(message, arguments);
#if DEBUG
                Debug.WriteLine(message, arguments);
#endif
            }
        }

        public void Information(string message)
        {
            Write(message);
        }

        public void Configuration(string source, string name, string value)
        {
        }

        public void Enter(string invocationId, object instance, string method, IDictionary<string, object> parameters)
        {
            Write("{0} - [{1}]: Entered method {2} with arguments: {3}", invocationId, instance, method, parameters.AsFormattedString());
        }

        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            Write("{0} - {1}", invocationId, request.AsString());
        }

        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            Write("{0} - {1}", invocationId, response.AsString());
        }

        public void Error(string invocationId, Exception ex)
        {
            Write("{0} - Error: {1}", invocationId, ex);
        }

        public void Exit(string invocationId, object result)
        {
            Write("{0} - Exited method with result: {1}", invocationId, result);
        }
    }
}
