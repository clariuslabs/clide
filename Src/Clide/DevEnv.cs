#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Entry point to the Clide developer environment APIs.
    /// </summary>
    public static class DevEnv
    {
        private static Lazy<DevEnvFactory> defaultFactory = new Lazy<DevEnvFactory>(() => new DevEnvFactory());
        private static AmbientSingleton<Func<IServiceProvider, IDevEnv>> devEnvFactory =
            new AmbientSingleton<Func<IServiceProvider, IDevEnv>>(services => defaultFactory.Value.Get(services));

        static DevEnv()
        {
            Tracer.Initialize(new TracerManager());

#if DEBUG
            Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, SourceLevels.All);
#else
            Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, SourceLevels.Warning);
#endif

            if (Debugger.IsAttached)
                Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, SourceLevels.All);
        }

        /// <summary>
        /// Gets the developer environment API root.
        /// </summary>
        /// <remarks>
        /// By default, the <see cref="IDevEnv"/> instance is cached for the 
        /// AppDomain, and only created once, using the given 
        /// IDE services as necessary. This default behavior can 
        /// be overriden by setting the <see cref="DevEnvFactory"/>.
        /// </remarks>
        public static IDevEnv Get(IServiceProvider services)
        {
            return devEnvFactory.Value.Invoke(services);
        }

        /// <summary>
        /// Gets or sets the factory that will create instances of 
        /// <see cref="IDevEnv"/> when the <see cref="Get"/> method 
        /// is invoked by consumers. This is an ambient singleton, so 
        /// it is safe to replace it in multi-threaded test runs.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Func<IServiceProvider, IDevEnv> DevEnvFactory
        {
            get { return devEnvFactory.Value; }
            set { devEnvFactory.Value = value; }
        }
    }
}
