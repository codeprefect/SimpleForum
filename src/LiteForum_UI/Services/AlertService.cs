using Microsoft.AspNetCore.Components;
using LiteForum_UI.Models;
using System;
using Microsoft.AspNetCore.Components.Routing;

namespace LiteForum_UI.Services
{
    public class AlertService : IAlertService, IDisposable
    {
        protected NavigationManager _navigationManager { get; }
        protected EventHandler<AlertMessage> AlertReceived { get; set; }

        public AlertService(NavigationManager navManager)
        {
            _navigationManager = navManager;
            _navigationManager.LocationChanged += OnLocationChanges;
        }

        public void OnLocationChanges(object sender, LocationChangedEventArgs args) => AlertReceived.Invoke(this, null); // trigger to remove stale alerts

        public void Success(string message, bool keepAfterNavChange = false) =>
            AlertReceived.Invoke(this, new AlertMessage(AlertType.Success, message, keepAfterNavChange));

        public void Warning(string message, bool keepAfterNavChange = false) =>
            AlertReceived.Invoke(this, new AlertMessage(AlertType.Warning, message, keepAfterNavChange));

        public void Error(string message, bool keepAfterNavChange = false) =>
            AlertReceived.Invoke(this, new AlertMessage(AlertType.Error, message, keepAfterNavChange));

        public void OnAlertReceived(AlertMessage alert) {
            var handler = AlertReceived;
            if(handler != null) handler(this, alert);
        }

        public void Dispose()
        {
            _navigationManager.LocationChanged -= OnLocationChanges;
        }

        public void AddAlertReceivedHandler(EventHandler<AlertMessage> handler) => AlertReceived += handler;
        public void RemoveAlertReceivedHandler(EventHandler<AlertMessage> handler) => AlertReceived -= handler;
    }
}
