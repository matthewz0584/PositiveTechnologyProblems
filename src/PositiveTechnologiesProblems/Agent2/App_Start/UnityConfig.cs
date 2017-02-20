using System;
using System.Threading.Tasks.Dataflow;
using System.Web.Http;
using log4net;
using Microsoft.Practices.Unity;
using PositiveTechnologiesProblems.Domain;
using Unity.WebApi;

namespace PositiveTechnologiesProblems.Agent2
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
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(10, typeof (Func<FibonacciState, IFibonacciSequence>), new FibonacciState(1), new FibonacciState(1)));

            container.RegisterInstance(LogManager.GetLogger(""));
            container.RegisterInstance<IOutboundTransport>(new OutboundHttpTransport("http://localhost.fiddler:21005/"));
            container.RegisterType<MessagingChainBuilder>(new ContainerControlledLifetimeManager());

            container.RegisterType<ITargetBlock<UpdateMessageDto>>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(
                    ioc => ioc.Resolve<MessagingChainBuilder>()
                            .BuildMessagingChain(ioc.Resolve<IFibonacciDistributedSequencesManager>())));
            
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}