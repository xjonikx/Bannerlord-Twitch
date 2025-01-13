using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets;
using Microsoft.Extensions.Hosting;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace BannerlordTwitch.Twitch
{
    public class TwitchEventSubSocket : IHostedService
    {
        public delegate void ChannelPointsRewardEvent(object e, ChannelPointsCustomRewardRedemption args);
        public delegate void SubWebSocketConnectedEvent(object e, WebsocketConnectedArgs args);
        private readonly ILogger<TwitchEventSubSocket> _logger;
        private readonly EventSubWebsocketClient _eventSubWebsocketClient;

        public SubWebSocketConnectedEvent OnEventSubServiceConnected;
        public ChannelPointsRewardEvent OnChannelPointsRewardsRedeemed;

        public string SessionId { 
            get{
                return _eventSubWebsocketClient?.SessionId;
            } 
        }

        public TwitchEventSubSocket(ILogger<TwitchEventSubSocket> logger = null)
        {
            _logger = logger;
            _eventSubWebsocketClient = new EventSubWebsocketClient(null);

            _eventSubWebsocketClient.WebsocketConnected += OnWebsocketConnected;
            _eventSubWebsocketClient.WebsocketDisconnected += OnWebsocketDisconnected;
            _eventSubWebsocketClient.WebsocketReconnected += OnWebsocketReconnected;
            _eventSubWebsocketClient.ErrorOccurred += OnErrorOccurred;

            _eventSubWebsocketClient.ChannelPointsCustomRewardRedemptionAdd += async (object e, ChannelPointsCustomRewardRedemptionArgs args) =>
            {
                OnChannelPointsRewardsRedeemed.Invoke(e, args.Notification.Payload.Event);
            };
  
            _eventSubWebsocketClient.ChannelFollow += OnChannelFollow;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _eventSubWebsocketClient.DisconnectAsync();
        }

        private async Task OnErrorOccurred(object sender, ErrorOccuredArgs e)
        {
        }

        private async Task OnChannelFollow(object sender, ChannelFollowArgs e)
        {
            var eventData = e.Notification.Payload.Event;
        }

        private async Task OnWebsocketConnected(object sender, WebsocketConnectedArgs e)
        {
            if (!e.IsRequestedReconnect)
            {
                OnEventSubServiceConnected.Invoke(sender, e);
                // subscribe to topics
            }
        }

        private async Task OnWebsocketDisconnected(object sender, EventArgs e)
        {
            // Don't do this in production. You should implement a better reconnect strategy
            while (!await _eventSubWebsocketClient.ReconnectAsync())
            {
                await Task.Delay(1000);
            }
        }

        private async Task OnWebsocketReconnected(object sender, EventArgs e)
        {
        }
    }
}
