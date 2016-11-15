using DarkSkyApi;
using DarkSkyApi.Models;
using Lottery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace VKfunctions
{
    class VK
    {
        public static long? lastMesIdVK;
        public static long? lastPostIdVKFKN;
        public static long? lastPostIdVKSBS;

        public static VkApi authVK()
        {
            var vkBot = new VkApi();
            FileStream file1 = new FileStream("..\\auth.txt", FileMode.Open); //создаем файловый поток
            StreamReader reader = new StreamReader(file1); // создаем «потоковый читатель» и связываем его с файловым потоком 
            string json = reader.ReadToEnd(); //считываем все данные с потока и выводим на экран
            reader.Close(); //закрываем поток
            AuthInfo Auth = JsonConvert.DeserializeObject<AuthInfo>(json);
            Settings scope = Settings.All;     // Приложение имеет доступ ко всему
            try
            {
                vkBot.Authorize(new ApiAuthParams { ApplicationId = (ulong)Auth.vkAppId, Login = Auth.vkLogin, Password = Auth.vkPass, Settings = scope });
                vkBot.Account.SetOnline(true);
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][Auth] " + ex.Message, ConsoleColor.DarkRed);
                Thread.Sleep(5000);
            }
            return vkBot;
        }

        public static async void getNewMessages()
        {
            VkApi vkBot = authVK();
            try
            {
                var vkMsgLast = vkBot.Messages.Get(new MessagesGetParams { Offset = 0, Count = 1 });
                if (vkMsgLast.Messages[0].Body != null)
                {
                    lastMesIdVK = vkMsgLast.Messages[0].Id;
                }
                else getNewMessages();
                while (true)
                {
                    var updinfVK = vkBot.Messages.Get(new MessagesGetParams { Offset = 0, Count = 200, LastMessageId = lastMesIdVK });
                    if (updinfVK.Messages.Count > 0) { lastMesIdVK = updinfVK.Messages[0].Id; }
                    foreach (var msg in updinfVK.Messages)
                    {
                        var userInfo = vkBot.Users.Get((long)msg.UserId);
                        if (msg.Body.Length != 0)
                        {
                            BotSBS.BODY.debugLine("[VK] " + userInfo.FirstName + " " + userInfo.LastName + ": " + msg.Body, ConsoleColor.White);
                            long chatId = (long)msg.UserId;
                            bool toUser = true;
                            if (msg.ChatId != null)
                            {
                                chatId = (long)msg.ChatId;
                                toUser = false;
                            }
                            //lottery sector
                            if (!toUser) Lot.addPointsByID((long)msg.UserId, (ulong)msg.Body.Length);
                            //
                            switch (msg.Body.Remove(0, 1).ToLower())
                            {
                                case "week": sendSchedule(chatId, "bothWeeks", msg.Id, false, toUser); break;
                                case "неделя": sendSchedule(chatId, "bothWeeks", msg.Id, false, toUser); break;
                                case "dr": sendMessage(chatId, "Дрисня", msg.Id, toUser); break;
                                case "др": sendMessage(chatId, "Дрисня", msg.Id, toUser); break;
                                case "evenweek": sendSchedule(chatId, "evenWeek", msg.Id, false, toUser); break;
                                case "четная": sendSchedule(chatId, "evenWeek", msg.Id, false, toUser); break;
                                case "oddweek": sendSchedule(chatId, "oddWeek", msg.Id, false, toUser); break;
                                case "нечетная": sendSchedule(chatId, "oddWeek", msg.Id, false, toUser); break;
                                case "tomorrow": sendSchedule(chatId, "tomorrow", msg.Id, false, toUser); break;
                                case "завтра": sendSchedule(chatId, "tomorrow", msg.Id, false, toUser); break;
                                case "today": sendSchedule(chatId, "today", msg.Id, false, toUser); break;
                                case "сегодня": sendSchedule(chatId, "today", msg.Id, false, toUser); break;
                                case "погода": sendWeather(chatId, msg.Id, toUser); break;
                                case "новость": sendNews(msg); break;
                                case "help": sendMessage(chatId, "Список команд:\n/сегодня - расписание на сегодня\n/завтра - расписание на завтра\n/неделя - расписание на текущую неделю\n/четная - расписание на четную неделю\n/нечетная - расписание на нечетную неделю\n/погода - прогноз погоды\n/новость - последняя новость из нашей группы", msg.Id, toUser); break;
                                case "cmds": sendMessage(chatId, "Список команд:\n/сегодня - расписание на сегодня\n/завтра - расписание на завтра\n/неделя - расписание на текущую неделю\n/четная - расписание на четную неделю\n/нечетная - расписание на нечетную неделю\n/погода - прогноз погоды\n/новость - последняя новость из нашей группы", msg.Id, toUser); break;
                                case "помощь": sendMessage(chatId, "Список команд:\n/сегодня - расписание на сегодня\n/завтра - расписание на завтра\n/неделя - расписание на текущую неделю\n/четная - расписание на четную неделю\n/нечетная - расписание на нечетную неделю\n/погода - прогноз погоды\n/новость - последняя новость из нашей группы", msg.Id, toUser); break;
                                case "top": sendMessage(chatId, Lot.getTopElements(5), msg.Id, toUser); break;
                                default: catchTheText(chatId, msg); break;
                            }
                        }
                    }
                    BotSBS.BODY.UpdateConsoleName();
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][getNewMessages] " + ex.Message, ConsoleColor.DarkRed);
                Thread.Sleep(5000);
                getNewMessages();
            }
        }

        public static async void getNewPosts()
        {
            VkApi vkBot = authVK();
            try
            {
                var vkPostsLastFKN = vkBot.Wall.Get(new WallGetParams { Domain = "fkn_news", Offset = 0, Count = 1 });
                if (vkPostsLastFKN.WallPosts.Count > 0) { lastPostIdVKFKN = vkPostsLastFKN.WallPosts[0].Id; }
                var vkPostsLast = vkBot.Wall.Get(new WallGetParams { Domain = "sbs_501_o", Offset = 0, Count = 1 });
                if (vkPostsLast.WallPosts.Count > 0) { lastPostIdVKSBS = vkPostsLast.WallPosts[0].Id; }
                while (true)
                {
                    await Task.Delay(2000);
                    var upinfVKFKN = vkBot.Wall.Get(new WallGetParams { Domain = "fkn_news", Offset = 0, Count = 1 });
                    if (upinfVKFKN.WallPosts.Count > 0)
                    {
                        if (lastPostIdVKFKN < upinfVKFKN.WallPosts[0].Id)
                        {
                            var userInfo = vkBot.Groups.GetById(Math.Abs((long)upinfVKFKN.WallPosts[0].OwnerId));
                            BotSBS.BODY.debugLine("[VK] " + userInfo.Name + ": " + upinfVKFKN.WallPosts[0].Text, ConsoleColor.White);
                            var node = new List<MediaAttachment> { upinfVKFKN.WallPosts[0] };
                            vkBot.Messages.Send(new MessagesSendParams { ChatId = 2, Attachments = node });
                            lastPostIdVKFKN = upinfVKFKN.WallPosts[0].Id;
                        }
                    }
                    await Task.Delay(2000);
                    var upinfVKSBS = vkBot.Wall.Get(new WallGetParams { Domain = "sbs_501_o", Offset = 0, Count = 1 });
                    if (upinfVKSBS.WallPosts.Count > 0)
                    {
                        if (lastPostIdVKSBS < upinfVKSBS.WallPosts[0].Id)
                        {
                            if (upinfVKSBS.WallPosts[0].Text.ToLower().StartsWith("#бот"))
                            {
                                var userInfo = vkBot.Groups.GetById(Math.Abs((long)upinfVKSBS.WallPosts[0].OwnerId));
                                BotSBS.BODY.debugLine("[VK] " + userInfo.Name + ": " + upinfVKSBS.WallPosts[0].Text, ConsoleColor.White);
                                vkBot.Likes.Add(new LikesAddParams { Type = LikeObjectType.Post, ItemId = (long)upinfVKSBS.WallPosts[0].Id, OwnerId = upinfVKSBS.WallPosts[0].OwnerId });
                                var node = new List<MediaAttachment> { upinfVKSBS.WallPosts[0] };
                                vkBot.Messages.Send(new MessagesSendParams { ChatId = 2, Attachments = node });
                            }
                            lastPostIdVKSBS = upinfVKSBS.WallPosts[0].Id;
                        }
                    }
                    BotSBS.BODY.UpdateConsoleName();
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][getNewPosts] " + ex.Message, ConsoleColor.DarkRed);
                await Task.Delay(5000);
                getNewPosts();
            }
        }

        public static string getFullNameByID(long id)
        {
            VkApi vkBot = authVK();
            return vkBot.Users.Get(id).LastName.ToString() + " " + vkBot.Users.Get(id).FirstName.ToString();
        }

        public static async void sendNews(VkNet.Model.Message msg)
        {
            VkApi vkBot = authVK();
            try
            {
                var upinfVKSBS = vkBot.Wall.Get(new WallGetParams { Domain = "sbs_501_o", Offset = 0, Count = 10 });
                foreach (var post in upinfVKSBS.WallPosts)
                {
                    if (post.Text.ToLower().StartsWith("#бот"))
                    {
                        var userInfo = vkBot.Groups.GetById(Math.Abs((long)post.OwnerId));
                        BotSBS.BODY.debugLine("[VK] " + userInfo.Name + ": " + post.Text, ConsoleColor.White);
                        var node = new List<MediaAttachment> { post };
                        vkBot.Messages.Send(new MessagesSendParams { ChatId = msg.ChatId, Attachments = node });
                        break;
                    }
                }
                BotSBS.BODY.UpdateConsoleName();
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][sendNews] " + ex.Message, ConsoleColor.DarkRed);
                await Task.Delay(5000);
            }
        }

        public static async void sendSchedule(long chatid, string type, long? messageId, bool weatherState, bool toUser)
        {
            long[] msgId = { (long)messageId };
            if (!toUser)
            {
                chatid += 2000000000;
            }
            try
            {
                var weather = "";
                if (weatherState) { weather = "Погода \n в 08:00: " + BotSBS.BODY.weather2h + "\n в 13:00: " + BotSBS.BODY.weather7h + "\n\n"; }
                VkApi vkBot = authVK();
                vkBot.Messages.Send(new MessagesSendParams { PeerId = chatid, Message = weather + BotSBS.BODY.getSchedule(type), ForwardMessages = msgId });
                BotSBS.BODY.debugLine("[VK] Schedule with type " + type + " has been sent.", ConsoleColor.DarkGreen);
                await Task.Delay(5000);
            }
            catch (VkNet.Exception.CaptchaNeededException)
            {
                BotSBS.BODY.debugLine("[VK][sendSchedule] Captcha", ConsoleColor.DarkRed);
                await Task.Delay(20000);
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][sendSchedule] " + ex.Message, ConsoleColor.DarkRed);
                await Task.Delay(3000);
            }
        }

        public static async void sendMessage(long chatid, string message, long? messageId, bool toUser)
        {
            if (!toUser)
            {
                chatid += 2000000000;
            }
            long[] msgId = { (long)messageId };
            try
            {
                VkApi vkBot = authVK();
                vkBot.Messages.Send(new MessagesSendParams { PeerId = chatid, Message = message, ForwardMessages = msgId });
                BotSBS.BODY.debugLine("[VK] Message with text '" + message + "' has been sent.", ConsoleColor.DarkGreen);
                await Task.Delay(2000);
            }
            catch (VkNet.Exception.CaptchaNeededException)
            {
                BotSBS.BODY.debugLine("[VK][sendMessage] Captcha", ConsoleColor.DarkRed);
                await Task.Delay(20000);
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][sendMessage] " + ex.Message, ConsoleColor.DarkRed);
                await Task.Delay(3000);
            }
        }

        public static async void sendWeather(long chatid, long? messageId, bool toUser)
        {
            if (!toUser)
            {
                chatid += 2000000000;
            }
            try
            {
                var client = new DarkSkyService("1faf41da033a5161112cd61890341d1e");
                Forecast result = await client.GetWeatherDataAsync(54.966047, 73.2871069, Unit.SI, Language.Russian);
                var message = "Сейчас: " + (Math.Round(result.Currently.Temperature, 0)).ToString() + "°С " + result.Currently.Summary;
                message += "\n Через 2 ч. будет " + (Math.Round(result.Hourly.Hours[2].Temperature, 0)).ToString() + "°С | " + result.Hourly.Hours[2].Summary;
                message += "\n Через 4 ч. будет " + (Math.Round(result.Hourly.Hours[4].Temperature, 0)).ToString() + "°С | " + result.Hourly.Hours[4].Summary;
                message += "\n Через 6 ч. будет " + (Math.Round(result.Hourly.Hours[6].Temperature, 0)).ToString() + "°С | " + result.Hourly.Hours[6].Summary;
                message += "\n Через 8 ч. будет " + (Math.Round(result.Hourly.Hours[8].Temperature, 0)).ToString() + "°С | " + result.Hourly.Hours[8].Summary;
                message += "\n Завтра днем будет " + (Math.Round(result.Daily.Days[1].MaxTemperature, 0)).ToString() + "°С | " + result.Daily.Days[1].Summary;

                long[] msgId = { (long)messageId };
                try
                {
                    VkApi vkBot = authVK();
                    vkBot.Messages.Send(new MessagesSendParams { PeerId = chatid, Message = message, ForwardMessages = msgId });
                    BotSBS.BODY.debugLine("[VK] " + "Weather has been sent", ConsoleColor.DarkGreen);
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    BotSBS.BODY.debugLine("[VK][sendWeather] " + ex.Message, ConsoleColor.DarkRed);
                    await Task.Delay(3000);
                }
            } catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][sendWeather] " + ex.Message, ConsoleColor.DarkRed);
                await Task.Delay(3000);
            }
        }

        public static void catchTheText(long chatid, VkNet.Model.Message message)
        {
            long chatId = (long)message.UserId;
            bool toUser = true;
            if (message.ChatId != null)
            {
                chatId = (long)message.ChatId;
                toUser = false;
            }
            message.Body = message.Body.ToLower();

            if (message.Body.Contains(" добре") || message.Body.StartsWith("добре") || message.Body.StartsWith("доброе") || message.Body.StartsWith(" доброе") || message.Body.StartsWith("добрый") || message.Body.StartsWith(" добрый"))
            {
                if (BotSBS.BODY.calculateRandom(0.8))
                {
                    sendMessage(chatid, BotSBS.BODY.getRandomPhrase("Добре"), message.Id, toUser);
                }
            }
            else if (message.Body.Contains(" кек") || message.Body.StartsWith("кек") || message.Body.Contains(" лол") || message.Body.StartsWith("лол") || message.Body.Contains("хах") || message.Body.Contains("хех"))
            {
                if (BotSBS.BODY.calculateRandom(0.1))
                {
                    sendMessage(chatid, BotSBS.BODY.getRandomPhrase("Кек"), message.Id, toUser);
                }
            }
            else if (message.Body.Contains(" оооо") || message.Body.StartsWith("оооо"))
            {
                if (BotSBS.BODY.calculateRandom(0.8))
                {
                    sendMessage(chatid, BotSBS.BODY.getRandomPhrase("ооо"), message.Id, toUser);
                }
            }
        }
    }
}