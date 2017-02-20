using System;
using Microsoft.Practices.Unity;
using System.Web.Http;
using PositiveTechnologies;
using Unity.WebApi;

namespace Agent2
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            container.RegisterInstance<Func<FibonacciState, IFibonacciSequence>>(
                //initState => new FibonacciSkipOneSequence(initState));
                initState => new FibonacciSequence(initState));
            container.RegisterType<IFibonacciDistributedSequencesManager, FibonacciDistributedSequencesManager>(
                new ContainerControlledLifetimeManager(), new InjectionConstructor(10, typeof(Func<FibonacciState, IFibonacciSequence>), new FibonacciState(1), new FibonacciState(1)));
            
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}