﻿/*
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace GrpcDotNetNamedPipes.Tests.Helpers;

public abstract class ChannelContextFactory
{
    public abstract ChannelContext Create(ITestOutputHelper output = null);
    public abstract ChannelContext WarmCreate(ITestOutputHelper output = null, int port = 0);
    public abstract TestService.TestServiceClient CreateClient(ITestOutputHelper output = null);
}