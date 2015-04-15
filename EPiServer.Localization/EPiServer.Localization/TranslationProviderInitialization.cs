﻿using System;
using System.Collections.Specialized;
using System.Linq;

using EPiServer.Events;
using EPiServer.Events.Clients;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;

using log4net;
using EPiServer;
using EPiServer.Localization.Models;

namespace EPiServer.Localization
{
	/// <summary>
	///     The initialization module for the translation provider.
	/// </summary>
	[InitializableModule]
	[ModuleDependency(typeof(FrameworkInitialization))]
	public class TranslationProviderInitialization : IInitializableModule
	{
		#region Constants

		/// <summary>
		///     The provider name.
		/// </summary>
		private const string ProviderName = "Translations";

		#endregion

		#region Static Fields

		// Generate unique id for the reload event.
		private static readonly Guid EventId = new Guid("9674113d-5135-49ff-8d2b-80ee6ae8f9e9");

		/// <summary>
		///     Initializes the <see cref="LogManager">LogManager</see> for the <see cref="TranslationProviderInitialization" />
		///     class.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(typeof(TranslationProviderInitialization));

		//Generate unique id for the raiser.
		private static readonly Guid RaiserId = new Guid("cb4e20de-5dd3-48cd-b72a-0ecc3ce08cee");

		/// <summary>
		///     A synchronize lock.
		/// </summary>
		private static readonly object SyncLock = new object();

		#endregion

		#region Fields

		/// <summary>
		///     The localization service
		/// </summary>
		private ProviderBasedLocalizationService localizationService;

		#endregion

		#region Properties

		/// <summary>
		///     Gets the localization service.
		/// </summary>
		/// <value>
		///     The localization service.
		/// </value>
		private ProviderBasedLocalizationService LocalizationService
		{
			get
			{
				return this.localizationService ?? (this.localizationService = GetLocalizationService());
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		/// <param name="context">
		///     The context.
		/// </param>
		/// <remarks>
		///     Gets called as part of the EPiServer Framework initialization sequence. Note that it will be called
		///     only once per AppDomain, unless the method throws an exception. If an exception is thrown, the initialization
		///     method will be called repeatedly for each request reaching the site until the method succeeds.
		/// </remarks>
		public void Initialize(InitializationEngine context)
		{
			// If there is no context, we can't do anything.
			if (context == null)
				return;

			Log.Info("[Localization] Initializing translation provider.");

			// Initialize the provider after the initialization is complete, else the StartPage cannot be found.
			context.InitComplete += this.InitComplete;

			// Attach events to update the translations when a translation or container is published, moved or deleted.
			DataFactory.Instance.PublishedPage += this.InstanceChangedPage;
			DataFactory.Instance.MovedPage += this.InstanceChangedPage;
			DataFactory.Instance.DeletedPage += this.InstanceChangedPage;

			// Attach a custom event to update the translations when translations are updated, eg. in LoadBalanced environments.
			Event translationsUpdated = Event.Get(EventId);
			translationsUpdated.Raised += this.TranslationsUpdatedEventRaised;

			Log.Info("[Localization] Translation provider initialized.");
		}

		/// <summary>
		///     Preloads the module.
		/// </summary>
		/// <param name="parameters">
		///     The parameters.
		/// </param>
		/// <remarks>
		///     This method is only available to be compatible with "AlwaysRunning" applications in .NET 4 / IIS 7.
		///     It currently serves no purpose.
		/// </remarks>
		public void Preload(string[] parameters)
		{
		}

		/// <summary>
		///     Resets the module into an uninitialized state.
		/// </summary>
		/// <param name="context">
		///     The context.
		/// </param>
		/// <remarks>
		///     <para>
		///         This method is usually not called when running under a web application since the web app may be shut down very
		///         abruptly, but your module should still implement it properly since it will make integration and unit testing
		///         much simpler.
		///     </para>
		///     <para>
		///         Any work done by
		///         <see
		///             cref="M:EPiServer.Framework.IInitializableModule.Initialize(EPiServer.Framework.Initialization.InitializationEngine)" />
		///         as well as any code executing on
		///         <see cref="E:EPiServer.Framework.Initialization.InitializationEngine.InitComplete" />
		///         should be reversed.
		///     </para>
		/// </remarks>
		public void Uninitialize(InitializationEngine context)
		{
			// If there is no context, we can't do anything.
			if (context == null)
				return;

			Log.Info("[Localization] Uninitializing translation provider.");

			TranslationProvider translationProvider = this.GetTranslationProvider();

			this.UnLoadProvider(translationProvider);

			Log.Info("[Localization] Translation provider uninitialized.");
		}

		#endregion

		#region Methods

		/// <summary>
		///     Run when initialization is complete.
		/// </summary>
		/// <param name="sender">
		///     The sender.
		/// </param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		internal void InitComplete(object sender, EventArgs e)
		{
			this.LoadProvider();
		}

		/// <summary>
		///     Gets the localization service.
		/// </summary>
		/// <returns>
		///     The current <see cref="ProviderBasedLocalizationService" />.
		/// </returns>
		private static ProviderBasedLocalizationService GetLocalizationService()
		{
			ProviderBasedLocalizationService service;

			try
			{
				// Casts the current LocalizationService to a ProviderBasedLocalizationService to get access to the current list of providers.
				service = ServiceLocator.Current.GetInstance<LocalizationService>() as ProviderBasedLocalizationService;
			}
			catch (ActivationException)
			{
				return null;
			}

			return service;
		}

		/// <summary>
		///     Load the translations.
		/// </summary>
		/// <param name="localizationProvider">
		///     The localization Provider.
		/// </param>
		private static void LoadTranslations(TranslationProvider localizationProvider)
		{
			if (localizationProvider != null)
			{
				localizationProvider.LoadTranslations();
				return;
			}

			Log.Info("[Localization] Translation provider not found, no translations loaded.");
		}

		/// <summary>
		///     Get the localization provider.
		/// </summary>
		/// <returns>
		///     The <see cref="LocalizationProvider" />.
		/// </returns>
		private TranslationProvider GetTranslationProvider()
		{
			if (this.LocalizationService == null)
			{
				return null;
			}

			// Gets any provider that has the same name as the one initialized.
			LocalizationProvider localizationProvider =
				this.LocalizationService.Providers.FirstOrDefault(
					p => p.Name.Equals(ProviderName, StringComparison.Ordinal));

			return localizationProvider as TranslationProvider;
		}

		/// <summary>
		///     If a translation gets published, moved or deleted, update the provider.
		/// </summary>
		/// <param name="sender">
		///     Source of the event.
		/// </param>
		/// <param name="e">
		///     Page event information.
		/// </param>
		private void InstanceChangedPage(object sender, PageEventArgs e)
		{
			if (e == null)
			{
				return;
			}

			if (!(e.Page is TranslationContainer) && !(e.Page is TranslationItem)
				&& !(e.Page is CategoryTranslationContainer))
			{
				return;
			}

			this.ReloadProvider();

			RaiseEvent("[Localization] Translation updated.");
		}

		/// <summary>
		///     Loads the provider.
		/// </summary>
		/// <returns>
		///     [true] if the provider has been loaded.
		/// </returns>
		private bool LoadProvider()
		{
			if (this.LocalizationService == null)
			{
				return false;
			}

			// This config value could tell the provider where to find the translations, 
			// set to 0 though, will be looked up after initialization in the provider itself.
			NameValueCollection configValues = new NameValueCollection { { "containerid", "0" } };

			TranslationProvider translationProviderProvider = new TranslationProvider();

			// Instantiate the provider
			translationProviderProvider.Initialize(ProviderName, configValues);

			// Add it at the end of the list of providers.
			try
			{
				this.LocalizationService.Providers.Add(translationProviderProvider);
			}
			catch (NotSupportedException notSupportedException)
			{
				Log.Error("[Localization] Error add provider to the Localization Service.", notSupportedException);
				return false;
			}

			LoadTranslations(translationProviderProvider);

			return true;
		}

		private static void RaiseEvent(string message)
		{
			// Raise the TranslationsUpdated event.
			Event.Get(EventId).Raise(RaiserId, message);
		}

		/// <summary>
		///     Reloads the provider.
		/// </summary>
		private void ReloadProvider()
		{
			lock (SyncLock)
			{
				TranslationProvider translationProvider = this.GetTranslationProvider();

				this.UnLoadProvider(translationProvider);
				this.LoadProvider();
			}
		}

		/// <summary>
		///     Removes from cache event raised.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventNotificationEventArgs" /> instance containing the event data.</param>
		private void TranslationsUpdatedEventRaised(object sender, EventNotificationEventArgs e)
		{
			// We don't want to process events raised on this machine so we will check the raiser id.
			if (e.RaiserId == RaiserId)
			{
				return;
			}

			this.ReloadProvider();
			Log.Info("[Localization] Translations updated on other machine. Reloaded provider.");
		}

		/// <summary>
		///     Uns the load provider.
		/// </summary>
		/// <returns>
		///     [false] if the provider has been unloaded, as it's not initialized anymore.
		/// </returns>
		private bool UnLoadProvider(LocalizationProvider localizationProvider)
		{
			if (this.LocalizationService == null)
			{
				return false;
			}

			if (localizationProvider != null)
			{
				// If found, remove it.
				this.LocalizationService.Providers.Remove(localizationProvider);
			}

			return false;
		}

		#endregion
	}
}
