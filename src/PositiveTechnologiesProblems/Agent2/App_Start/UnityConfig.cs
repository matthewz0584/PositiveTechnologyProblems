using System;
using System.Configuration;
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
        //Registration of single looped back service 
        //public static void RegisterComponents()
        //{
        //    var container = new UnityContainer();

        //    container.RegisterInstance<Func<FibonacciState, IFibonacciSequence>>(
        //        initState => new FibonacciSequence(initState));
        //    container.RegisterType<IFibonacciDistributedSequencesManager, FibonacciDistributedSequencesManager>(
        //        new ContainerControlledLifetimeManager(),
        //        new InjectionConstructor(Convert.ToInt32(ConfigurationManager.AppSettings["SequencesNumber"]), typeof(Func<FibonacciState, IFibonacciSequence>), new FibonacciState(1), new FibonacciState(1)));

        //    container.RegisterInstance(LogManager.GetLogger(""));
        //    container.RegisterInstance<IOutboundTransport>(new OutboundHttpTransport(ConfigurationManager.AppSettings["PartnerUrl"]));
        //    container.RegisterType<MessagingChainBuilder>(new ContainerControlledLifetimeManager());

        //    container.RegisterType<ITargetBlock<UpdateMessageDto>>(new ContainerControlledLifetimeManager(),
        //        new InjectionFactory(
        //            ioc => ioc.Resolve<MessagingChainBuilder>()
        //                    .BuildMessagingChain(ioc.Resolve<IFibonacciDistributedSequencesManager>())));

        //    GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        //}

        //Registration of one partner
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            container.RegisterInstance<Func<FibonacciState, IFibonacciSequence>>(
                initState => new FibonacciSkipOneSequence(initState));
            container.RegisterType<IFibonacciDistributedSequencesManager, FibonacciDistributedSequencesManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(Convert.ToInt32(ConfigurationManager.AppSettings["SequencesNumber"]), typeof(Func<FibonacciState, IFibonacciSequence>), new FibonacciState(0), new FibonacciState(1)));

            container.RegisterInstance(LogManager.GetLogger(""));
            container.RegisterInstance<IOutboundTransport>(new OutboundHttpTransport(ConfigurationManager.AppSettings["PartnerUrl"]));
            container.RegisterType<MessagingChainBuilder>(new ContainerControlledLifetimeManager());

            container.RegisterType<ITargetBlock<UpdateMessageDto>>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(
                    ioc => ioc.Resolve<MessagingChainBuilder>()
                            .BuildMessagingChain(ioc.Resolve<IFibonacciDistributedSequencesManager>())));

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}