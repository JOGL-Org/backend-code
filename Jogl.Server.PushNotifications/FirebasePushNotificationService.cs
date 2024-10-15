using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace Jogl.Server.PushNotifications
{
    public class FirebasePushNotificationService : IPushNotificationService
    {
        private readonly IConfiguration _configuration;
        public FirebasePushNotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(_configuration["Google:Firebase"]),
            });
        }

        public async Task PushAsync(string token, string title, string body, string link)
        {
            var messaging = FirebaseMessaging.DefaultInstance;
            var imageUrl = _configuration["App:URL"] + "/images/pwa/android/android-launchericon-192-192.png";
            var badgeUrl = _configuration["App:URL"] + "/images/pwa/android/android-launchericon-96-96.png";

            var result = await messaging.SendAsync(new Message
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    //ImageUrl = imageUrl,
                },
                Token = token,
                Webpush = new WebpushConfig { FcmOptions = new WebpushFcmOptions { Link = link }, Notification = new WebpushNotification { Icon = imageUrl, Badge = badgeUrl } },
                Android = new AndroidConfig { Notification = new AndroidNotification { Icon = imageUrl } },
                Apns = new ApnsConfig { Aps = new Aps { Alert = new ApsAlert { Title = title, Body = body } } }
            });

            if (string.IsNullOrEmpty(result))
                throw new Exception($"Error sending push notification: {result}");
        }

        public async Task PushAsync(List<string> tokens, string title, string body, string link)
        {
            if (!tokens.Any())
                return;

            var messaging = FirebaseMessaging.DefaultInstance;
            var imageUrl = _configuration["App:URL"] + "/images/pwa/android/android-launchericon-192-192.png";
            var badgeUrl = _configuration["App:URL"] + "/images/pwa/android/android-launchericon-96-96.png";
            var result = await messaging.SendEachAsync(tokens.Select(token => new Message
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                    //ImageUrl = imageUrl,
                },
                Token = token,
                Webpush = new WebpushConfig { FcmOptions = new WebpushFcmOptions { Link = link }, Notification = new WebpushNotification { Icon = imageUrl, Badge = badgeUrl } },
                Android = new AndroidConfig { Notification = new AndroidNotification { Icon = imageUrl } },
                Apns = new ApnsConfig { Aps = new Aps { Alert = new ApsAlert { Title = title, Body = body } }, FcmOptions = new ApnsFcmOptions { ImageUrl = imageUrl } }
            }));
        }

        private Dictionary<string, string> GetDataDictionary(object data)
        {
            var res = new Dictionary<string, string>();
            foreach (var prop in data.GetType().GetProperties())
            {
                res.Add(prop.Name, prop.GetValue(data).ToString());
            }

            return res;
        }
    }
}