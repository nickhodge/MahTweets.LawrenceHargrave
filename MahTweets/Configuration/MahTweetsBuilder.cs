using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using System.Windows;
using Autofac;
using Autofac.Integration.Mef;
using AutofacContrib.DynamicProxy2;
using Castle.DynamicProxy;
using MahTweets.Core;
using MahTweets.Core.Commands;
using MahTweets.Core.Events;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Interfaces.ViewModels;
using MahTweets.Core.Scripting;
using MahTweets.Core.VersionCheck;
using MahTweets.Core.ViewModels;
using MahTweets.Helpers;
using MahTweets.Services;
using MahTweets.TweetProcessors;
using MahTweets.ViewModels;

namespace MahTweets.Configuration
{
    public class MahTweetsBuilder : ContainerBuilder
    {
        public MahTweetsBuilder(ComposablePartCatalog catalog)
        {
            Catalog = catalog;

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Assembly coreAssembly = typeof (IEventAggregator).Assembly;

            // defaults - find interfaces and concrete types and marry them up
            this.RegisterAssemblyTypes(currentAssembly, coreAssembly)
                .AsImplementedInterfaces();

            this.RegisterType<AccountSettingsProvider>()
                .As<IAccountSettingsProvider>()
                .SingleInstance();

            this.RegisterType<ColumnsSettingsProvider>()
                .As<IColumnsSettingsProvider>()
                .SingleInstance();

            this.RegisterType<ApplicationSettingsProvider>()
                .As<IApplicationSettingsProvider>()
                .Exported(x => x.As<IApplicationSettingsProvider>()) // make this part visible to MEF components
                .SingleInstance();

            this.RegisterType<PluginSettingsProvider>()
                .As<IPluginSettingsProvider>()
                .Exported(x => x.As<IPluginSettingsProvider>())
                .SingleInstance();

            this.RegisterType<XmlSettingsProvider>()
                .As<ISettingsProvider>()
                .SingleInstance();

            this.RegisterType<EventAggregator>()
                .As<IEventAggregator>()
                .Exported(x => x.As<IEventAggregator>()) // make this part visible to MEF components
                .SingleInstance();

            this.RegisterType<PluginRepository>()
                .As<IPluginRepository>()
                .Exported(x => x.As<IPluginRepository>())
                .SingleInstance();

            this.RegisterType<LongUrlPleaseService>()
                .As<IUrlExpandService>()
                .SingleInstance();

            this.RegisterType<ManualLongUrlRetrieverService>()
                .As<IManualLongUrlRetrieverService>()
                .InstancePerDependency();

            this.RegisterType<Storage>()
                .As<IStorage>()
                .SingleInstance();

            this.RegisterType<TextProcessorEngine>()
                .As<ITextProcessorEngine>()
                .SingleInstance();

            this.RegisterType<CurrentVersion>()
                .As<ICurrentVersionCheck>()
                .SingleInstance();

            this.RegisterType<AutoNotifyPropertyChangedInterceptor>();

            this.RegisterAssemblyTypes(coreAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .EnableClassInterceptors()
                .InterceptedBy(typeof (AutoNotifyPropertyChangedInterceptor));

            this.RegisterAssemblyTypes(currentAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .EnableClassInterceptors()
                .InterceptedBy(typeof (AutoNotifyPropertyChangedInterceptor));

            this.RegisterType<AppViewModel>()
                .SingleInstance();

            this.RegisterType<MainViewModel>()
                .As<IMainViewModel>()
                .SingleInstance()
                .EnableClassInterceptors()
                .InterceptedBy(typeof (AutoNotifyPropertyChangedInterceptor))
                .OnActivated(a =>
                                 {
                                     var viewModel = a.Instance;
                                     // commands that are not dependent on external members
                                     viewModel.AddColumnCommand = new DelegateCommand(viewModel.CreateBlankStream);
                                     viewModel.SearchCommand = new DelegateCommand(viewModel.CreateNewSearch);
                                     viewModel.RefreshCommand = new DelegateCommand(viewModel.Refresh);
                                     viewModel.ClearAllCommand = new DelegateCommand(viewModel.ClearAll);
                                     viewModel.AboutCommand = new DelegateCommand(viewModel.ShowAbout);
                                     viewModel.SetupCommand = new DelegateCommand(viewModel.ShowSetup);
                                     viewModel.NewUpdateCommand = new DelegateCommand(viewModel.NewUpdate);
                                     // are we not using the commands here
                                     viewModel.CloseCommand = new DelegateCommand(viewModel.Close);
                                     viewModel.SendMessageCommand =
                                         new SendMessageCommand((obj, src) => viewModel.SendMessage(obj));
                                 });

            this.RegisterType<StreamViewModel>()
                .EnableClassInterceptors()
                .InterceptedBy(typeof (AutoNotifyPropertyChangedInterceptor))
                .OnActivated(a =>
                                 {
                                     var viewmodel = a.Instance;
                                     viewmodel.ClearCommand =
                                         new DelegateCommand(() => viewmodel.ClearBeforeDate(DateTime.Now));
                                 });


            this.RegisterType<SetupViewModel>()
                .SingleInstance();

            this.RegisterType<ResourcesViewModel>()
                .As<IResourcesViewModel>()
                .SingleInstance();

            this.RegisterType<HashtagService>()
                .AsImplementedInterfaces()
                .SingleInstance();

            this.RegisterType<ContactService>()
                .AsImplementedInterfaces()
                .Exported(x => x.As<IContactsRepository>()) // make this part visible to MEF components
                .SingleInstance();

            this.RegisterType<PluginLoader>()
                .As<IPluginLoader>()
                .SingleInstance();

            // StatusUpdateManager defined under two interfaces
            // one for plugins to use, the other for application to use
            var statusManager = new StatusUpdateService();

            this.RegisterType<StatusUpdateService>()
                .AsImplementedInterfaces()
                .Exported(x => x.As<IStatusUpdateRepository>()) // make this part visible to MEF components
                .SingleInstance();

            // register MEF plugins for consumption
             this.RegisterComposablePartCatalog(catalog);

            // default behaviour - just use the damn window
            this.RegisterType<MainWindow>()
                .As<MainWindow>()
                .As<IShell>()
                .SingleInstance();

            this.RegisterInstance(Application.Current.Resources)
                .ExternallyOwned()
                .SingleInstance();

            // MahTweets Scripting
            // order is important

            this.RegisterType<ScriptingManager>()
                .As<IScriptingManager>()
                .SingleInstance();

            this.RegisterType<ScriptingConfiguration>()
                .As<IScriptingConfiguration>()
                .SingleInstance();

            this.RegisterType<ScriptingHelper>()
                .As<IScriptingHelper>()
                .SingleInstance();

            this.RegisterType<ScriptingUIHelper>()
                .As<IScriptingUIHelper>()
                .SingleInstance();

            this.RegisterType<DynamicLanguagesScriptEngine>()
                .As<IScriptingEngine>()
                .SingleInstance();

            this.RegisterType<ScriptLibrarian>()
                .As<IScriptingLibrarian>()
                .SingleInstance();
        }

        public ComposablePartCatalog Catalog { get; private set; }
    }

    public class AutoNotifyPropertyChangedInterceptor : IInterceptor
    {
        private const string SetPrefix = "set_";

        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            if (!invocation.Method.Name.StartsWith(SetPrefix)) return;
            if (!(invocation.Proxy is BaseViewModel)) return;

            MethodInfo methodInfo = invocation.Method;
            var model = (BaseViewModel) invocation.Proxy;
            model.OnPropertyChanged(methodInfo.Name.Substring(SetPrefix.Length));

            ChangeNotificationForDependentProperties(methodInfo, model);
        }

        #endregion

        private void ChangeNotificationForDependentProperties(MethodInfo methodInfo, BaseViewModel model)
        {
            if (NoAdditionalProperties(methodInfo)) return;

            var properties = GetAdditionalPropertiesToChangeNotify(methodInfo);

            foreach (var propertyName in properties)
                model.OnPropertyChanged(propertyName);
        }

        private bool NoAdditionalProperties(MethodInfo methodInfo)
        {
            var pi = GetPropertyInfoForSetterMethod(methodInfo);
            return (pi == null || pi.GetAttribute<NotifyChangeForAttribute>() == null);
        }

        private IEnumerable<string> GetAdditionalPropertiesToChangeNotify(MethodInfo methodInfo)
        {
            var pi = GetPropertyInfoForSetterMethod(methodInfo);
            var attribute = pi.GetAttribute<NotifyChangeForAttribute>();
            return attribute.NotifyChangeFor;
        }

        private PropertyInfo GetPropertyInfoForSetterMethod(MethodInfo methodInfo)
        {
            var propertyName = methodInfo.Name.Substring(SetPrefix.Length);
            if (methodInfo.DeclaringType != null) return methodInfo.DeclaringType.GetProperty(propertyName);
            return null;
        }
    }

    public class NotifyChangeForAttribute : Attribute
    {
        public NotifyChangeForAttribute(params string[] notifyChangeFor)
        {
            NotifyChangeFor = notifyChangeFor;
        }

        public string[] NotifyChangeFor { get; private set; }
    }
}