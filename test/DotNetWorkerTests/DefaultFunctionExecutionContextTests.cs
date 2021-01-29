﻿using System;
using Microsoft.Azure.Functions.Worker.Context;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Azure.Functions.Worker.Tests
{
    public class DefaultFunctionExecutionContextTests
    {
        DefaultFunctionExecutionContext _defaultFunctionExecutionContext;
        IServiceScopeFactory _serviceScopeFactory;
        IServiceProvider _serviceProvider;

        public DefaultFunctionExecutionContextTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<SingletonService>();
            serviceCollection.AddTransient<TransientService>();
            serviceCollection.AddScoped<ScopedService>();
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _serviceScopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

            var invocation = new Mock<FunctionInvocation>(MockBehavior.Strict).Object;
            var definition = new Mock<FunctionDefinition>(MockBehavior.Strict).Object;

            _defaultFunctionExecutionContext = new DefaultFunctionExecutionContext(_serviceScopeFactory, invocation, definition);
        }

        [Fact]
        public void CreateAndDisposeInstanceServicesTest()
        {
            var services = _defaultFunctionExecutionContext.InstanceServices;
            Assert.NotNull(services);
            var singletonService = services.GetService<SingletonService>();
            var transientService = services.GetService<TransientService>();
            var scopedService = services.GetService<ScopedService>();
            Assert.NotNull(scopedService);
            Assert.NotNull(transientService);
            Assert.NotNull(singletonService);

            _defaultFunctionExecutionContext.Dispose();

            Assert.Throws<ObjectDisposedException>(services.GetService<SingletonService>);
            Assert.Throws<ObjectDisposedException>(services.GetService<TransientService>);
            Assert.Throws<ObjectDisposedException>(services.GetService<ScopedService>);
            Assert.True(scopedService.IsDisposed);
            Assert.True(transientService.IsDisposed);
            Assert.False(singletonService.IsDisposed);
        }

        // service classes for testing
        private class SingletonService : IDisposable
        {
            public SingletonService() { }
            public bool IsDisposed { get; private set; }
            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class TransientService : SingletonService { }

        private class ScopedService : SingletonService { }
    }
}
